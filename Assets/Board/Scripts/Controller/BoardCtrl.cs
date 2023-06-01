using System;
using System.Collections.Generic;
using Board.Model;
using Board.View;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Board.Presenter
{
    public readonly struct BoardRectInfo
    {
        private readonly RectTransform _coordMarker;
        private readonly int _rows;
        private readonly int _cols;
        private readonly float _paddingWidth;
        private readonly Dictionary<BoardVec2, Vector3> _worldPosMap;

        public readonly float blockRectSize;
        
        public BoardRectInfo(int rows, int cols, RectTransform boardRectTransform)
        {
            var canvasWidth = boardRectTransform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
            _paddingWidth = canvasWidth * 0.05f;
            blockRectSize = (canvasWidth - _paddingWidth) / 9f;
            var boardRectWidth = _paddingWidth + blockRectSize * cols;
            var boardRectHeight = _paddingWidth + blockRectSize * rows;
            boardRectTransform.sizeDelta = new Vector2(boardRectWidth, boardRectHeight);

            _rows = rows;
            _cols = cols;

            _coordMarker = new GameObject("marker", typeof(RectTransform)).GetComponent<RectTransform>();
            _worldPosMap = new Dictionary<BoardVec2, Vector3>();
            InitCoordMarker(boardRectTransform);
            InitWorldPosMap();
        }

        private void InitCoordMarker(RectTransform parent)
        {
            _coordMarker.SetParent(parent);
            _coordMarker.anchorMin = _coordMarker.anchorMax = _coordMarker.pivot = Vector2.up;
            _coordMarker.localPosition = Vector3.zero;
            _coordMarker.SetWidth(10f);
            _coordMarker.SetHeight(10f);
        }

        private void InitWorldPosMap()
        {
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    var idx = new BoardVec2(y, x);
                    _coordMarker.anchoredPosition = GetBlockAnchoredPosAt(idx);
                    _worldPosMap[idx] = _coordMarker.position;
                }
            }
        }

        public Vector2 GetBlockAnchoredPosAt(BoardVec2 array2dIdx)
        {
            var posX = _paddingWidth * 0.5f + blockRectSize * array2dIdx.X;
            var posY = _paddingWidth * 0.5f + blockRectSize * array2dIdx.Y;
            return new Vector2(posX, -posY);
        }

        public Vector3 GetBlockWorldPosAt(BoardVec2 array2dIdx) => _worldPosMap[array2dIdx];
    }
    public class BoardCtrl: MonoBehaviour
    {
        [SerializeField] private GameObject _blockRPrefab;
        [SerializeField] private GameObject _blockGPrefab;
        [SerializeField] private GameObject _blockBPrefab;
        [SerializeField] private GameObject _blockYPrefab;
        [SerializeField] private RectTransform _boardRectTransform;

        public int rows, cols;

        private BoardData _boardData;
        private Block[,] _blocks;
        private readonly Dictionary<Block, BlockData> _blockDataMap = new();
        private BoardRectInfo _boarRectInfo;
        private MatchFinder _matchFinder;

        public void InitBoardData(BoardData boardData)
        {
            _boardData = new BoardData(boardData);
            _boarRectInfo = new BoardRectInfo(boardData.Rows, boardData.Cols, _boardRectTransform);
            _blocks = new Block[boardData.Rows, boardData.Cols];
            _matchFinder = new MatchFinder(boardData);

            BlockData.PopulateBlockType(_boardData.BlockDataArray2D);
            
            VerifyBoard();
            
        }

        [ButtonMethod()]
        public void InitBoardData()
        {
            _boardData = new BoardData(rows, cols);
            _boarRectInfo = new BoardRectInfo(rows, cols, _boardRectTransform);
            _blocks = new Block[rows, cols];
            _matchFinder = new MatchFinder(_boardData);

            BlockData.PopulateBlockType(_boardData.BlockDataArray2D);
            
            VerifyBoard();
        }

        private void VerifyBoard()
        {
            var matchedBlocksData = _matchFinder.GetMatchedBlocksData();
            while (matchedBlocksData.Count > 0)
            {
                BlockData.PopulateBlockType(matchedBlocksData);
                matchedBlocksData = _matchFinder.GetMatchedBlocksData();
            }
        }

        [ButtonMethod()]
        public void CreateBoardAndFillBlock()
        {
            for (int y = 0; y < _boardData.Rows; y++)
            {
                for (int x = 0; x < _boardData.Cols; x++)
                {
                    var idx = new BoardVec2(y, x);
                    var blockPrefab = _boardData.GetCurBlockTypeAt(idx) switch
                    {
                        BlockType.Red => _blockRPrefab,
                        BlockType.Green => _blockGPrefab,
                        BlockType.Blue => _blockBPrefab,
                        BlockType.Yellow => _blockYPrefab,
                        _ => _blockRPrefab
                    };

                    var block = CreateBlock(blockPrefab, idx);
                    block.draggedBlock += OnDraggedBlock;
                    _blockDataMap[block] = _boardData.GetBlockDataAt(idx);
                    _blocks[y, x] = block;
                }
            }
        }

        [ButtonMethod()]
        public void Reset()
        {
            for (var i = _boardRectTransform.childCount - 1; i >= 0 ; i--)
            {
                DestroyImmediate(_boardRectTransform.GetChild(i).gameObject);
            }
        }

        private Block CreateBlock(GameObject blockPrefab, BoardVec2 array2dIdx)
        {
            var blockObj = Instantiate(blockPrefab, _boardRectTransform);
            blockObj.name = array2dIdx;

            var rect = blockObj.GetComponent<RectTransform>();
            rect.anchoredPosition = _boarRectInfo.GetBlockAnchoredPosAt(array2dIdx);
            rect.sizeDelta = Vector2.one * _boarRectInfo.blockRectSize;

            return blockObj.GetComponent<Block>();
        }

        private void OnDraggedBlock(Block grabbedBlock, BoardVec2 toDir)
        {
            var destIdx = _blockDataMap[grabbedBlock].array2dIdx + toDir;
            var destBlock = GetBlockAt(destIdx);
            var cantSwap = (destBlock == null) ||
                           !IsSwappableBlock(_blockDataMap[grabbedBlock], _blockDataMap[destBlock]);
            if (cantSwap)
                return;
            SwapDataBetween(grabbedBlock, destBlock);
            UpdateBlockView(grabbedBlock, destBlock);
        }

        private Block GetBlockAt(BoardVec2 array2dIdx)
        {
            if (array2dIdx.Y < 0 || array2dIdx.Y >= _boardData.Rows ||
                array2dIdx.X < 0 || array2dIdx.X >= _boardData.Cols)
                return null;
            return _blocks[array2dIdx.Y, array2dIdx.X];
        }

        private bool IsSwappableBlock(BlockData blockA, BlockData blockB)
        {
            return blockA.currentType != BlockType.None && blockB.currentType != BlockType.None &&
                   blockA.state != BlockState.Moving && blockB.state != BlockState.Moving;
        }

        private void SwapDataBetween(Block blockA, Block blockB)
        {
            var idxA = _blockDataMap[blockA].array2dIdx;
            var idxB = _blockDataMap[blockB].array2dIdx;

            _boardData.SetBlockDataAt(idxA, _blockDataMap[blockB]);
            _boardData.SetBlockDataAt(idxB, _blockDataMap[blockA]);

            SetBlocksAt(idxA, blockB);
            SetBlocksAt(idxB, blockA);

            (_blockDataMap[blockA].array2dIdx, _blockDataMap[blockB].array2dIdx) = (idxB, idxA);
        }

        private void SetBlocksAt(BoardVec2 array2dIdx, Block block) => _blocks[array2dIdx.Y, array2dIdx.X] = block;

        private void UpdateBlockView(Block blockA, Block blockB, bool cancelSwap = false)
        {
            var idxA = _blockDataMap[blockA].array2dIdx;
            var idxB = _blockDataMap[blockB].array2dIdx;

            blockA.name = idxA;
            blockB.name = idxB;

            var posA = _boarRectInfo.GetBlockWorldPosAt(idxA); 
            var posB = _boarRectInfo.GetBlockWorldPosAt(idxB); 
            var seq = DOTween.Sequence();
            seq.Join(blockA.transform.DOMove(posA, 0.5f)).
                Join(blockB.transform.DOMove(posB, 0.5f)).
                OnComplete(OnSwappedDone(blockA, blockB, cancelSwap));
        }

        private TweenCallback OnSwappedDone(Block blockA, Block blockB, bool cancelSwap) =>
            () =>
            {
                if (cancelSwap)
                    return;
                if (_matchFinder.GetMatchedBlocksData().Count > 0)
                {
                    return;
                }
                SwapDataBetween(blockA, blockB);
                UpdateBlockView(blockA,blockB,true);
            };
    }
}
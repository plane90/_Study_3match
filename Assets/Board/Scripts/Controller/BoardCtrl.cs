using System.Collections.Generic;
using Board.Editor;
using Board.Model;
using Board.View;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Board.Presenter
{
    public class BoardCtrl: MonoBehaviour
    {
        [SerializeField] private GameObject _blockRPrefab;
        [SerializeField] private GameObject _blockGPrefab;
        [SerializeField] private GameObject _blockBPrefab;
        [SerializeField] private GameObject _blockYPrefab;
        [SerializeField] private RectTransform _boardRectTransform;

        public int rows, cols;

        public BoardData boardData;
        private Block[,] _blocks;
        private readonly Dictionary<Block, BlockData> _blockDataMap = new();
        private BoardRectInfo _boarRectInfo;
        private MatchFinder _matchFinder;

        public void InitBoardData(BoardData boardData)
        {
            this.boardData = new BoardData(boardData);
            _boarRectInfo = new BoardRectInfo(boardData.Rows, boardData.Cols, _boardRectTransform);
            _blocks = new Block[boardData.Rows, boardData.Cols];
            _matchFinder = new MatchFinder(boardData);
            
            VerifyBoard();
        }

        [ButtonMethod()]
        public void InitBoardData()
        {
            boardData = new BoardData(rows, cols);
            _boarRectInfo = new BoardRectInfo(rows, cols, _boardRectTransform);
            _blocks = new Block[rows, cols];
            _matchFinder = new MatchFinder(boardData);
            
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
            for (int y = 0; y < boardData.Rows; y++)
            {
                for (int x = 0; x < boardData.Cols; x++)
                {
                    var idx = new BoardVec2(y, x);
                    var blockPrefab = boardData.GetCurBlockTypeAt(idx) switch
                    {
                        BlockType.Red => _blockRPrefab,
                        BlockType.Green => _blockGPrefab,
                        BlockType.Blue => _blockBPrefab,
                        BlockType.Yellow => _blockYPrefab,
                        _ => _blockRPrefab
                    };

                    var block = CreateBlock(blockPrefab, idx);
                    block.draggedBlock += OnDraggedBlock;
                    _blockDataMap[block] = boardData.GetBlockDataAt(idx);
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
            var grabbedBlockData = _blockDataMap[grabbedBlock];
            var destIdx = grabbedBlockData.array2dIdx + toDir;
            var destBlock = GetBlockAt(destIdx);
            var cantSwap = (destBlock == null) ||
                           !IsSwappableBlock(grabbedBlockData, _blockDataMap[destBlock]);
            if (cantSwap)
            {
                UpdateBlockViewCantSwap(grabbedBlock, toDir);
                return;
            }

            SwapDataBetween(grabbedBlock, destBlock);
            UpdateBlockViewSwap(grabbedBlock, destBlock);
        }

        private Block GetBlockAt(BoardVec2 array2dIdx)
        {
            if (array2dIdx.Y < 0 || array2dIdx.Y >= boardData.Rows ||
                array2dIdx.X < 0 || array2dIdx.X >= boardData.Cols)
                return null;
            return _blocks[array2dIdx.Y, array2dIdx.X];
        }

        private bool IsSwappableBlock(BlockData blockA, BlockData blockB) =>
            blockA.currentType != BlockType.None && blockB.currentType != BlockType.None &&
            blockA.state == BlockState.Enable && blockB.state == BlockState.Enable;
        
        
        private void UpdateBlockViewCantSwap(Block block, BoardVec2 toDir)
        {
            var worldPos = _boarRectInfo.GetBlockWorldPosAt(_blockDataMap[block].array2dIdx);
            if (block.transform.position != worldPos)
                return;
            var punchPos = new Vector2(toDir.X, -toDir.Y) * 10f;
            block.transform.DOPunchPosition(punchPos, 0.5f).OnComplete(() =>
            {
                block.transform.position = worldPos;
            });
        }

        private void SwapDataBetween(Block blockA, Block blockB)
        {
            var idxA = _blockDataMap[blockA].array2dIdx;
            var idxB = _blockDataMap[blockB].array2dIdx;

            boardData.SetBlockDataAt(idxA, _blockDataMap[blockB]);
            boardData.SetBlockDataAt(idxB, _blockDataMap[blockA]);

            SetBlocksAt(idxA, blockB);
            SetBlocksAt(idxB, blockA);

            (_blockDataMap[blockA].array2dIdx, _blockDataMap[blockB].array2dIdx) = (idxB, idxA);
            
            _blockDataMap[blockA].state = BlockState.Updating;
            _blockDataMap[blockB].state = BlockState.Updating;
        }

        private void SetBlocksAt(BoardVec2 array2dIdx, Block block) => _blocks[array2dIdx.Y, array2dIdx.X] = block;

        private void UpdateBlockViewSwap(Block blockA, Block blockB, bool cancelSwap = false)
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
                _blockDataMap[blockA].state = BlockState.Enable;
                _blockDataMap[blockB].state = BlockState.Enable;
                if (cancelSwap)
                    return;
                var matchedBlocksData = _matchFinder.GetMatchedBlocksData();
                if (matchedBlocksData.Count > 0)
                {
                    foreach (var blockData in matchedBlocksData)
                    {
                        blockData.state = BlockState.Disable;
                    }
                    return;
                }

                SwapDataBetween(blockA, blockB);
                UpdateBlockViewSwap(blockA, blockB, true);
            };
    }
}
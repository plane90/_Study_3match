using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        private BoardData _boardData;
        private Block[,] _blocks;
        private readonly Dictionary<Block, BlockData> _blockDataMap = new();
        private BoardRectInfo _boarRectInfo;
        private MatchFinder _matchFinder;

        public BoardData GetBoardData() => _boardData;
        
        public void InitBoardData(BoardData boardData)
        {
            _boardData = new BoardData(boardData);
            _matchFinder = new MatchFinder(_boardData);
            _boarRectInfo = new BoardRectInfo(boardData.Rows, boardData.Cols, _boardRectTransform);
            _blocks = new Block[boardData.Rows, boardData.Cols];
            
            VerifyBoard();
        }

        [ButtonMethod()]
        public void InitBoardData()
        {
            _boardData = new BoardData(rows, cols);
            _boarRectInfo = new BoardRectInfo(rows, cols, _boardRectTransform);
            _blocks = new Block[rows, cols];
            _matchFinder = new MatchFinder(_boardData);
            
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
            var grabbedBlockData = _blockDataMap[grabbedBlock];
            var destIdx = grabbedBlockData.array2dIdx + toDir;
            var destBlock = GetBlockAt(destIdx);
            
            if (destBlock == null)
            {
                UpdateBlockViewOutOfBound(grabbedBlock, toDir);
                return;
            }

            var destBlockData = _blockDataMap[destBlock];
            if (!IsSwappableBlock(grabbedBlockData, destBlockData))
                return;

            SwapDataBetween(grabbedBlockData, destBlockData);
            UpdateBlockViewSwap(grabbedBlockData, destBlockData);
        }

        private Block GetBlockAt(BoardVec2 array2dIdx)
        {
            if (array2dIdx.Y < 0 || array2dIdx.Y >= _boardData.Rows ||
                array2dIdx.X < 0 || array2dIdx.X >= _boardData.Cols)
                return null;
            return _blocks[array2dIdx.Y, array2dIdx.X];
        }

        private bool IsSwappableBlock(BlockData blockA, BlockData blockB) =>
            blockA.currentType != BlockType.None && blockB.currentType != BlockType.None &&
            blockA.state == BlockState.Enable && blockB.state == BlockState.Enable;
        
        
        private void UpdateBlockViewOutOfBound(Block block, BoardVec2 toDir)
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

        private void SwapDataBetween(BlockData blockDataA, BlockData blockDataB)
        {
            _boardData.SetBlockDataAt(blockDataA.array2dIdx, blockDataB);
            _boardData.SetBlockDataAt(blockDataB.array2dIdx, blockDataA);

            (blockDataA.array2dIdx, blockDataB.array2dIdx) = (blockDataB.array2dIdx, blockDataA.array2dIdx);
        }

        private void UpdateBlockViewSwap(BlockData blockDataA, BlockData blockDataB, bool cancelSwap = false)
        {
            blockDataA.state = BlockState.Updating;
            blockDataB.state = BlockState.Updating;
            
            var blockA = GetBlockAt(blockDataB.array2dIdx);
            var blockB = GetBlockAt(blockDataA.array2dIdx);
            SetBlocksAt(blockDataA.array2dIdx, blockA);
            SetBlocksAt(blockDataB.array2dIdx, blockB);

            blockA.name = blockDataA.array2dIdx;
            blockB.name = blockDataB.array2dIdx;

            var posA = _boarRectInfo.GetBlockWorldPosAt(blockDataA.array2dIdx); 
            var posB = _boarRectInfo.GetBlockWorldPosAt(blockDataB.array2dIdx); 
            var seq = DOTween.Sequence();
            seq.Join(blockA.transform.DOMove(posA, 0.5f)).
                Join(blockB.transform.DOMove(posB, 0.5f)).
                OnComplete(OnSwappedDone(blockDataA, blockDataB, cancelSwap));
        }

        private void SetBlocksAt(BoardVec2 array2dIdx, Block block) => _blocks[array2dIdx.Y, array2dIdx.X] = block;

        private TweenCallback OnSwappedDone(BlockData blockDataA, BlockData blockDataB, bool cancelSwap) =>
            () =>
            {
                blockDataA.state = blockDataB.state = BlockState.Enable;
                if (cancelSwap)
                    return;
                var hasMatch = CheckMatch(blockDataA) | CheckMatch(blockDataB);
                if (hasMatch)
                    return;

                SwapDataBetween(blockDataA, blockDataB);
                UpdateBlockViewSwap(blockDataA, blockDataB, true);
            };

        private bool CheckMatch(BlockData blockData)
        {
            bool isMatch = false;
            if (_matchFinder.IsMatchAt(blockData, out var matches))
            {
                isMatch = true;
                foreach (var matchedBlockData in matches)
                {
                    matchedBlockData.state = BlockState.Disable;
                    GetBlockAt(matchedBlockData.array2dIdx).gameObject.SetActive(false);
                }
            }
            return isMatch;
        }

        private void ApplyGravity()
        {
            for (int x = 0; x < _boardData.Cols; x++)
            {
                for (int y = _boardData.Rows - 1; y >= 0; y--)
                {
                    var curBlockData = _boardData.BlockDataArray2D[y, x];
                    if (!IsDisableBlockData(curBlockData)) 
                        continue;
                    Debug.Log($"Disable Block Data Found, {y},{x}");
                    //yield return new WaitForSeconds(1.0f);
                    for (int ny = y - 1; ny >= 0; ny--)
                    {
                        var idxNext = new BoardVec2(ny, x);
                        var nextBlockData = _boardData.GetBlockDataAt(idxNext);
                        if (!IsGravityTarget(nextBlockData))
                            continue;
                        Debug.Log($"Gravity Target Block Data Found, {ny},{x}");
                        //yield return new WaitForSeconds(1.0f);
                        SwapDataBetween(curBlockData, nextBlockData);
                        UpdateBlockViewGravity(curBlockData, nextBlockData);
                        break;
                    }
                }
            }
        }

        private void UpdateBlockViewGravity(BlockData fromData, BlockData toData)
        {
            Debug.Log($"From: {fromData.array2dIdx}, to: {toData.array2dIdx}");
            var fromBlock = GetBlockAt(fromData.array2dIdx);
            var toBlock = GetBlockAt(toData.array2dIdx);
            SetBlocksAt(fromData.array2dIdx, toBlock);
            SetBlocksAt(toData.array2dIdx, fromBlock);
            
            fromBlock.name = fromData.array2dIdx;
            toData.state = BlockState.Updating;
            fromBlock.transform.DOMove(_boarRectInfo.GetBlockWorldPosAt(toData.array2dIdx), 0.5f).OnComplete((() =>
            {
                if (toData.state == BlockState.Disable)
                    return;
                toData.state = BlockState.Enable;
                CheckMatch(fromData);
                CheckMatch(toData);
            }));
        }

        private static bool IsDisableBlockData(BlockData nextBlockData)
        {
            return nextBlockData.currentType != BlockType.None &&
                   nextBlockData.state == BlockState.Disable;
        }

        private static bool IsGravityTarget(BlockData blockData)
        {
            return blockData.currentType != BlockType.None &&
                   blockData.state != BlockState.Disable;
        }
    }
}
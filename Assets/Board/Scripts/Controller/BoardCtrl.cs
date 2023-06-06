using System;
using System.Collections.Generic;
using System.Linq;
using Board.Editor;
using Board.Model;
using Board.View;
using DG.Tweening;
using MyBox;
using MyBox.EditorTools;
using UnityEngine;
using UnityEngine.Assertions;
using LogType = Board.Editor.LogType;
using Random = UnityEngine.Random;

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
        private Dictionary<MatchShape, BoardVec2> _hintsDataMap;
        
        [SerializeField] private RectTransform rect;
        private HashSet<MatchShape> _hintsShape;

        public BoardData GetBoardData() => _boardData;
        
        public void InitBoardData(BoardData boardData)
        {
            _boardData = new BoardData(boardData);
            _matchFinder = new MatchFinder(_boardData);
            _boarRectInfo = new BoardRectInfo(boardData.Rows, boardData.Cols, _boardRectTransform);
            _blocks = new Block[boardData.Rows, boardData.Cols];
            
            VerifyAlreadyMatch(out _);
        }

        [ButtonMethod()]
        public void InitBoardData()
        {
            _boardData = new BoardData(rows, cols);
            _boarRectInfo = new BoardRectInfo(rows, cols, _boardRectTransform);
            _blocks = new Block[rows, cols];
            _matchFinder = new MatchFinder(_boardData);
            
            VerifyAlreadyMatch(out _);
        }

        private void VerifyAlreadyMatch(out bool hasPopulated)
        {
            var matchedBlocksData = _matchFinder.GetMatchedBlocksData();
            if (matchedBlocksData.Count == 0)
            {
                hasPopulated = false;
                return;
            }
            while (matchedBlocksData.Count > 0)
            {
                BlockData.PopulateBlockType(matchedBlocksData);
                matchedBlocksData = _matchFinder.GetMatchedBlocksData();
            }
            hasPopulated = true;
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
                    block.droppedBlock += OnDroppedBlock;
                    _blockDataMap[block] = _boardData.GetBlockDataAt(idx);
                    _blocks[y, x] = block;
                }
            }

            VerifyNoMatch();
        }

        private void VerifyNoMatch()
        {
            SetHintMatchShapes();
            if (_hintsShape.Count == 0)
            {
                do
                {
                    Shuffle.Execute(_boardData.BlockDataArray2D, _blocks);
                    VerifyAlreadyMatch(out var hasPopulated);
                    if (hasPopulated)
                    {
                        SetHintMatchShapes();
                        if (_hintsShape.Count != 0)
                            break;
                        continue;
                    } 
                    SetHintMatchShapes();
                } while (_hintsShape.Count == 0);
                UpdateBlockViewShuffle();
            }
        }

        private void SetHintMatchShapes()
        {
            var boardDataClone = new BoardData(_boardData);
            var matchFinder = new MatchFinder(boardDataClone);
            var directions = new []{ BoardVec2.up, BoardVec2.right, BoardVec2.down, BoardVec2.left };
            _hintsDataMap = new Dictionary<MatchShape, BoardVec2>();
            _hintsShape = new HashSet<MatchShape>();
            for (int y = 0; y < boardDataClone.Rows; y++)
            {
                for (int x = 0; x < boardDataClone.Cols; x++)
                {
                    var curIdx = new BoardVec2(y, x);
                    var curData = boardDataClone.GetBlockDataAt(curIdx);
                    foreach (var dir in directions)
                    {
                        var nextIdx = curIdx + dir;
                        if (nextIdx.Y < 0 || nextIdx.Y >= boardDataClone.Rows || nextIdx.X < 0 || nextIdx.X >= boardDataClone.Cols)
                            continue;
                        var nextData = boardDataClone.GetBlockDataAt(curIdx + dir);
                        
                        var prevCurData = curData.array2dIdx;
                        var prevNextData = nextData.array2dIdx;
                        (curData.array2dIdx, nextData.array2dIdx) = (prevNextData, prevCurData);
                        boardDataClone.SetBlockDataAt(prevCurData, nextData);
                        boardDataClone.SetBlockDataAt(prevNextData, curData);

                        if (matchFinder.IsMatchAt(curIdx, out var candidates))
                        {
                            candidates.ForEach((x) =>
                            {
                                _hintsShape.Add(x);
                                _hintsDataMap[x] = curIdx;
                            });
                        }
                        
                        (curData.array2dIdx, nextData.array2dIdx) = (prevCurData, prevNextData);
                        boardDataClone.SetBlockDataAt(prevCurData, curData);
                        boardDataClone.SetBlockDataAt(prevNextData, nextData);
                    }
                }
            }
        }

        private void UpdateBlockViewShuffle()
        {
            var seq = DOTween.Sequence();
            foreach (var blockData in _boardData.BlockDataArray2D)
            {
                var block = GetBlockAt(blockData.array2dIdx);
                block.name = blockData.array2dIdx;
                var color = blockData.currentType switch
                {
                    BlockType.Red => Color.red,
                    BlockType.Green => Color.green,
                    BlockType.Blue => Color.blue,
                    BlockType.Yellow => Color.yellow,
                    _ => Color.red
                };
                block.SetLooks(color);
                var pos = _boarRectInfo.GetBlockWorldPosAt(blockData.array2dIdx);
                seq.Join(block.transform.DOMove(pos, 2.0f)).OnComplete(() => blockData.state = BlockState.Enable);
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
        
        [ButtonMethod()]
        public void ResetAndRecreateBoard()
        {
            Reset();
            InitBoardData();
            CreateBoardAndFillBlock();
        }
        
        [ButtonMethod()]
        public void OpenBoardDebugger()
        {
            BoardDebugger.ShowWindow(this);
        }

        [ButtonMethod()]
        public void Test()
        {
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

            SetDataSwap(grabbedBlockData, destBlockData);
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
            blockA.state != BlockState.Disable && blockB.state != BlockState.Disable &&
            blockA.state != BlockState.Updating && blockB.state != BlockState.Updating;
        
        
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

        private void SetDataSwap(BlockData blockDataA, BlockData blockDataB)
        {
            var prevA = blockDataA.array2dIdx;
            var prevB = blockDataB.array2dIdx;
            
            (blockDataA.array2dIdx, blockDataB.array2dIdx) = (prevB, prevA);
            _boardData.SetBlockDataAt(prevA, blockDataB);
            _boardData.SetBlockDataAt(prevB, blockDataA);

            Assert.IsTrue(blockDataA.array2dIdx == prevB);
            Assert.IsTrue(blockDataB.array2dIdx == prevA);
            Assert.IsTrue(_boardData.GetBlockDataAt(prevA) == blockDataB);
            Assert.IsTrue(_boardData.GetBlockDataAt(prevB) == blockDataA);

            var blockA = GetBlockAt(prevA);
            var blockB = GetBlockAt(prevB);
            SetBlocksAt(prevA, blockB);
            SetBlocksAt(prevB, blockA);
            
            Assert.IsTrue(GetBlockAt(prevA) == blockB);
            Assert.IsTrue(GetBlockAt(prevB) == blockA);
        }

        private void UpdateBlockViewSwap(BlockData blockDataA, BlockData blockDataB, bool cancelSwap = false)
        {
            blockDataA.state = BlockState.Updating;
            blockDataB.state = BlockState.Updating;
            
            var blockA = GetBlockAt(blockDataA.array2dIdx);
            var blockB = GetBlockAt(blockDataB.array2dIdx);

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
                var hasMatch = CheckMatchAndDisable(blockDataA) | CheckMatchAndDisable(blockDataB);
                if (hasMatch)
                    return;

                SetDataSwap(blockDataA, blockDataB);
                UpdateBlockViewSwap(blockDataA, blockDataB, true);
            };

        private bool CheckMatchAndDisable(BlockData blockData)
        {
            bool isMatch = false;
            if (_matchFinder.IsMatchAt(blockData, out HashSet<BlockData> matchedBlocksData))
            {
                isMatch = true;
                if (matchedBlocksData.Any((x) => x.state == BlockState.Updating))
                {
                    matchedBlocksData.
                        Where((x) => x.state == BlockState.Enable).
                        ForEach((x) => x.state = BlockState.Wait);
                }
                else
                {
                    UpdateBlockViewDisable(matchedBlocksData);
                }
            }
            return isMatch;
        }

        private void UpdateBlockViewDisable(IEnumerable<BlockData> toDisableData)
        {
            var seq = DOTween.Sequence();
            seq.Pause();
            BoardDebugger.Log($"Disable At {toDisableData.First().array2dIdx}", LogType.Disable);
            foreach (var blockData in toDisableData)
            {
                blockData.state = BlockState.Updating;
                seq.Join(GetBlockAt(blockData.array2dIdx).transform.DOScale(Vector3.zero, 0.2f));
            }
            seq.OnComplete(OnDisableDone(toDisableData));
            seq.Play();
        }

        private TweenCallback OnDisableDone(IEnumerable<BlockData> disabledData) =>
            () =>
            {
                BoardDebugger.Log($"Disable Done {disabledData.First().array2dIdx}", LogType.Disable);
                disabledData.ForEach((x) => x.state = BlockState.Disable);
                HideDisabledBlockAndDropOrPopulate();
            };

        private void HideDisabledBlockAndDropOrPopulate()
        {
            BoardDebugger.Indent(LogType.Disable);
            for (int x = 0; x < _boardData.Cols; x++)
            {
                int levelCnt = 0;
                BlockData prevDisabled = null;
                for (int y = _boardData.Rows - 1; y >= 0; y--)
                {
                    var disabled = _boardData.BlockDataArray2D[y, x];
                    if (!IsDisableBlockData(disabled)) 
                        continue;
                    
                    if (prevDisabled != null && prevDisabled != disabled)
                        levelCnt++;
                    var idxTop = new BoardVec2(-1 - levelCnt, disabled.array2dIdx.X);
                    GetBlockAt(disabled.array2dIdx).HideAt(_boarRectInfo.GetBlockAnchoredPosAt(idxTop));
                    prevDisabled = disabled;
                    
                    Debug.Log(idxTop);
                    
                    if (FindDropTargetData(y - 1, x, out var toDrop, out var isUpdating))
                    {
                        SetDataSwap(disabled, toDrop);
                        UpdateBlockViewDrop(toDrop);
                        BoardDebugger.Log($"Drop At {toDrop.array2dIdx}", LogType.Disable);
                    }
                    else
                    {
                        if (isUpdating)
                            break;
                        SetDataPopulate(disabled);
                        UpdateBlockViewPopulated(disabled);
                        BoardDebugger.Log($"Pop At {disabled.array2dIdx}", LogType.Disable);
                    }
                }
                BoardDebugger.Log($"", LogType.Disable);
            }
            BoardDebugger.Unindent(LogType.Disable);
        }

        private static bool IsDisableBlockData(BlockData blockData) =>
            blockData.currentType != BlockType.None &&
            blockData.state == BlockState.Disable;

        private bool FindDropTargetData(int idxRow, int idxCol, out BlockData toDropBlockData, out bool isUpdating)
        {
            for (int y = idxRow; y >= 0; y--)
            {
                var nextPos = new BoardVec2(y, idxCol);
                toDropBlockData = _boardData.GetBlockDataAt(nextPos);
                if (toDropBlockData.state == BlockState.Updating)
                {
                    isUpdating = true;
                    return false;
                }

                if (!IsDropTarget(toDropBlockData))
                    continue;

                isUpdating = false;
                return true;
            }
        
            toDropBlockData = null;
            isUpdating = false;
            return false;
        }

        private static bool IsDropTarget(BlockData blockData) =>
            blockData.currentType != BlockType.None &&
            blockData.state != BlockState.Disable;

        private void UpdateBlockViewDrop(BlockData toDropData)
        {
            var toDropBlock = GetBlockAt(toDropData.array2dIdx);
            
            toDropBlock.name = toDropData.array2dIdx;
            toDropData.state = BlockState.Updating;
            toDropBlock.Drop(_boarRectInfo.GetBlockAnchoredPosAt(toDropData.array2dIdx));
        }

        private void OnDroppedBlock(Block droppedBlock)
        {
            var dropped = _blockDataMap[droppedBlock];
            bool found = false;
            for (int y = dropped.array2dIdx.Y; y < _boardData.Rows; y++)
            {
                var nextPos = new BoardVec2(y, dropped.array2dIdx.X);
                var disabled = _boardData.GetBlockDataAt(nextPos);
                if (disabled.state != BlockState.Disable)
                    continue;
                SetDataSwap(dropped, disabled);
                UpdateBlockViewDrop(dropped);
                found = true;
            }
            
            if (found)
                return;
            droppedBlock.Stop();
            BoardDebugger.Log($"drop done {dropped.array2dIdx}", LogType.Disable);
            dropped.state = BlockState.Enable;
            CheckMatchAndDisable(dropped);
            HideDisabledBlockAndDropOrPopulate();
        }

        private void SetDataPopulate(BlockData toPopulateBlockData)
        {
            toPopulateBlockData.currentType = (BlockType)Random.Range((int)BlockType.Red, (int)BlockType.End);
        }
        
        private void UpdateBlockViewPopulated(BlockData populated)
        {
            var populatedBlock = GetBlockAt(populated.array2dIdx);
            var color = populated.currentType switch
            {
                BlockType.Red => Color.red,
                BlockType.Green => Color.green,
                BlockType.Blue => Color.blue,
                BlockType.Yellow => Color.yellow,
                _ => Color.white
            };
            populatedBlock.SetLooks(color);
            populatedBlock.Show();

            populatedBlock.name = populated.array2dIdx;
            populated.state = BlockState.Updating;
            populatedBlock.Drop(_boarRectInfo.GetBlockAnchoredPosAt(populated.array2dIdx));
        }
    }
}
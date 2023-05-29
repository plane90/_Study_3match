using System.Collections.Generic;
using System.Diagnostics;
using BlockSystem.Model;
using UnityEngine;

namespace BoardSystem
{
    public class MatchFinder : MonoBehaviour
    {
        private static readonly Vec2[] _directions = { Vec2.up, Vec2.right, Vec2.down, Vec2.left };
        private static BoardData _curBoardData;
        private static readonly HashSet<BlockData> _visited = new();

        public static HashSet<BlockData> GetMatchedBlocksData(BoardData boardData)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            _curBoardData = boardData;
            _visited.Clear();
            
            foreach (var blockData in boardData.BlockDataArray2D)
            {
                if (_visited.Contains(blockData))
                    continue;
                
                if (FindMatchLine(blockData))
                {
                    TraverseDfs(blockData);
                    continue;
                }

                if (FindMatchCube(blockData))
                    TraverseDfs(blockData);
            }
            
            stopWatch.Stop();
            UnityEngine.Debug.Log($"done: {stopWatch.Elapsed.TotalMilliseconds.ToString()}");
            
            return _visited;
        }

        private static bool FindMatchLine(BlockData blockData)
        {
            var targetType = blockData.blockType;
            foreach (var dir in _directions)
            {
                bool isMatch = true;
                for (int i = 1; i <= 2; i++)
                {
                    var nextPos = blockData.idxArray2D + dir * i;
                    if (!IsOutOfBound(nextPos) && 
                        _curBoardData.GetBlockTypeAt(nextPos) == blockData.blockType)
                    {
                        continue;
                    }
                    isMatch = false;
                    break;
                }

                if (isMatch)
                    return true;
            }
            
            return false;
        }

        private static bool FindMatchCube(BlockData blockData)
        {
            for (int start = 0; start < 4; start++)
            {
                bool isMatch = true;
                var nextPos = blockData.idxArray2D;
                for (int i = 0; i < 4; i++)
                {
                    nextPos += _directions[(start + i) % 4];
                    if (!IsOutOfBound(nextPos) && 
                        _curBoardData.GetBlockTypeAt(nextPos) == blockData.blockType)
                    {
                        continue;
                    }
                    isMatch = false;
                    break;
                }

                if (isMatch)
                    return true;
            }

            return false;
        }

        private static bool IsOutOfBound(Vec2 nextPos) => 
            nextPos.X < 0 || nextPos.X >= _curBoardData.Cols ||
            nextPos.Y < 0 || nextPos.Y >= _curBoardData.Rows;

        private static void TraverseDfs(BlockData blockData)
        {
            _visited.Add(blockData);
            foreach (var dir in _directions)
            {
                var nextPos = blockData.idxArray2D + dir;
                if (IsOutOfBound(nextPos) ||
                    _curBoardData.GetBlockTypeAt(nextPos) != blockData.blockType ||
                    _visited.Contains(_curBoardData.GetBlockDataAt(nextPos)))
                {
                    continue;
                }
                TraverseDfs(_curBoardData.GetBlockDataAt(nextPos));
            }
        }
        
        
    }
}
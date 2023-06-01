using System.Collections.Generic;
using Board.Model;

namespace Board
{
    public class MatchFinder
    {
        private static readonly BoardVec2[] Directions = { BoardVec2.up, BoardVec2.right, BoardVec2.down, BoardVec2.left };
        private readonly BoardData _boardData;
        private readonly HashSet<BlockData> _visited = new();

        public MatchFinder(BoardData boardData)
        {
            _boardData = boardData;
        }

        public HashSet<BlockData> GetMatchedBlocksData()
        {
            _visited.Clear();
            
            foreach (var blockData in _boardData.BlockDataArray2D)
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
            
            return _visited;
        }

        private bool FindMatchLine(BlockData blockData)
        {
            foreach (var dir in Directions)
            {
                bool isMatch = true;
                for (int i = 1; i <= 2; i++)
                {
                    var nextPos = blockData.array2dIdx + dir * i;
                    if (!IsOutOfBound(nextPos) && 
                        _boardData.GetCurBlockTypeAt(nextPos) == blockData.currentType)
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

        private bool FindMatchCube(BlockData blockData)
        {
            for (int start = 0; start < 4; start++)
            {
                bool isMatch = true;
                var nextPos = blockData.array2dIdx;
                for (int i = 0; i < 4; i++)
                {
                    nextPos += Directions[(start + i) % 4];
                    if (!IsOutOfBound(nextPos) && 
                        _boardData.GetCurBlockTypeAt(nextPos) == blockData.currentType)
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

        private bool IsOutOfBound(BoardVec2 nextPos) => 
            nextPos.X < 0 || nextPos.X >= _boardData.Cols ||
            nextPos.Y < 0 || nextPos.Y >= _boardData.Rows;

        private void TraverseDfs(BlockData blockData)
        {
            _visited.Add(blockData);
            foreach (var dir in Directions)
            {
                var nextPos = blockData.array2dIdx + dir;
                if (IsOutOfBound(nextPos) ||
                    _boardData.GetCurBlockTypeAt(nextPos) != blockData.currentType ||
                    _visited.Contains(_boardData.GetBlockDataAt(nextPos)))
                {
                    continue;
                }
                TraverseDfs(_boardData.GetBlockDataAt(nextPos));
            }
        }
        
    }
}
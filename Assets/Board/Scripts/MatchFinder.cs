using System.Collections.Generic;
using System.Linq;
using Board.Editor;
using Board.Model;
using LogType = Board.Editor.LogType;

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

        // 좌상단 블록부터 우하단 블록까지 모두 순회하여 연결 된 컴포넌트 탐색.
        public HashSet<BlockData> GetMatchedBlocksData()
        {
            _visited.Clear();
            BoardDebugger.ShowLog(false);
            foreach (var blockData in _boardData.BlockDataArray2D)
            {
                if (_visited.Contains(blockData))
                    continue;
                
                if (FindMatchLineOneSide(blockData, out _))
                {
                    TraverseDfs(blockData);
                    continue;
                }

                if (FindMatchCube(blockData, out _))
                    TraverseDfs(blockData);
            }
            
            BoardDebugger.ShowLog(true);
            return _visited;
        }
        
        public bool IsMatchAt(BlockData blockData, out HashSet<BlockData> resultMatchedBlockData)
        {
            resultMatchedBlockData = new HashSet<BlockData>();
            var candidatesMatchShape = new List<MatchShape>();
            /*
             * O O X O
             */
            if (FindMatchLineOneSide(blockData, out var matchLineOneSide))
            {
                candidatesMatchShape.Add(matchLineOneSide);
                /*
                 *   O
                 *   O
                 * O X O O
                 */
                FindMatchShapeOf(blockData, MatchShape.GetShapeMasksL(), ref candidatesMatchShape, LogType.FindMatchL);
            }

            /*
             * O X O O
             *   O
             */
            if (FindMatchLineBothSide(blockData, out var matchLineBothSide))
            {
                candidatesMatchShape.Add(matchLineBothSide);
                /*
                 *   O
                 * O X O
                 *   O
                 *   O
                 */
                FindMatchShapeOf(blockData, MatchShape.GetShapeMasksT(), ref candidatesMatchShape, LogType.FindMatchT);
            }

            /*
             * O O
             * O X O
             */
            if (FindMatchCube(blockData, out var matchCube))
                candidatesMatchShape.Add(matchCube);

            if (candidatesMatchShape.Count == 0)
            {
                resultMatchedBlockData = null;
                return false;
            }

            var highValueShape = candidatesMatchShape.OrderBy((x) => x.value).Last();
            var logType = highValueShape.shapeType switch
            {
                ShapeType.L => LogType.FindMatchL,
                ShapeType.T => LogType.FindMatchT,
                ShapeType.I1 => LogType.FindMatchLineOneSide,
                ShapeType.I2 => LogType.FindMatchLineBothSide,
                ShapeType.Cube => LogType.FindMatchCube,
                _ => LogType.FindMatchLineOneSide
            };
            BoardDebugger.Log("high value", logType);
            foreach (var coord in highValueShape.shapeMask.coord)
            {
                resultMatchedBlockData.Add(_boardData.GetBlockDataAt(coord));
            }
            return true;
        }

        private void FindMatchShapeOf(BlockData blockData, List<ShapeMask> shapeMasks, ref List<MatchShape> matchShapes, LogType logType)
        {
            BoardDebugger.Log(blockData.array2dIdx, logType);
            BoardDebugger.Indent(logType);
            foreach (var shapeMask in shapeMasks)
            {
                bool isMatch = true;
                foreach (var nextCoord in shapeMask.coord)
                {
                    var nextPos = blockData.array2dIdx + nextCoord;
                    BoardDebugger.Log(nextPos, logType);
                    if (!IsOutOfBound(nextPos) &&
                        _boardData.GetBlockStateAt(nextPos) != BlockState.Disable &&
                        _boardData.GetCurBlockTypeAt(nextPos) == blockData.currentType)
                        continue;

                    isMatch = false;
                }

                BoardDebugger.Log("", logType);
                if (isMatch)
                {
                    BoardDebugger.Log("match !", logType);
                    BoardDebugger.Unindent(logType);
                    var coord = GetAppliedShapeMask(blockData, shapeMask);
                    matchShapes.Add(new MatchShape(new ShapeMask(coord), logType == LogType.FindMatchL ? ShapeType.L : ShapeType.T));
                }
            }
            BoardDebugger.Log("no match", logType);
            BoardDebugger.Unindent(logType);
        }

        private static BoardVec2[] GetAppliedShapeMask(BlockData blockData, ShapeMask shapeMask)
        {
            var coord = new BoardVec2[1 + shapeMask.coord.Length];
            for (int i = 0; i < shapeMask.coord.Length; i++)
            {
                coord[i] = blockData.array2dIdx + shapeMask.coord[i];
            }
            coord[shapeMask.coord.Length] = blockData.array2dIdx;
            return coord;
        }

        /*
         * O O X O
         */
        private bool FindMatchLineOneSide(BlockData blockData, out MatchShape matchShape)
        {
            foreach (var dir in Directions)
            {
                BoardDebugger.Log(blockData.array2dIdx, LogType.FindMatchLineOneSide);
                BoardDebugger.Indent(LogType.FindMatchLineOneSide);
                bool isMatch = true;
                
                for (int i = 1; i <= 2; i++)
                {
                    var nextPos = blockData.array2dIdx + dir * i;
                    BoardDebugger.Log(nextPos, LogType.FindMatchLineOneSide);
                    
                    if (!IsOutOfBound(nextPos) && 
                        _boardData.GetBlockStateAt(nextPos) != BlockState.Disable &&
                        _boardData.GetCurBlockTypeAt(nextPos) == blockData.currentType)
                        continue;
                    
                    isMatch = false;
                    break;
                }
                BoardDebugger.Unindent(LogType.FindMatchLineOneSide);
                
                if (!isMatch) 
                    continue;
                
                BoardDebugger.Log("match !", LogType.FindMatchLineOneSide);
                var coord = new BoardVec2[]
                {
                    blockData.array2dIdx,
                    blockData.array2dIdx + dir,
                    blockData.array2dIdx + dir * 2
                };
                matchShape = new MatchShape(new ShapeMask(coord), ShapeType.I1);
                return true;
            }

            BoardDebugger.Log("no match", LogType.FindMatchLineOneSide);
            matchShape = new MatchShape(new ShapeMask(null), ShapeType.I1);
            return false;
        }

        /*
         * O X O O
         *   O
         */
        private bool FindMatchLineBothSide(BlockData blockData, out MatchShape matchShape)
        {
            BoardDebugger.Log(blockData.array2dIdx, LogType.FindMatchLineBothSide);
            BoardDebugger.Indent(LogType.FindMatchLineBothSide);
            
            var horizontalMatches = new List<BoardVec2>();
            horizontalMatches.Add(blockData.array2dIdx);
            FindMatchToward(blockData, BoardVec2.left, ref horizontalMatches);
            FindMatchToward(blockData, BoardVec2.right, ref horizontalMatches);
            
            
            var verticalMatches = new List<BoardVec2>();
            verticalMatches.Add(blockData.array2dIdx);
            FindMatchToward(blockData, BoardVec2.up, ref verticalMatches);
            FindMatchToward(blockData, BoardVec2.down, ref verticalMatches);
            BoardDebugger.Unindent(LogType.FindMatchLineBothSide);

            if (horizontalMatches.Count < 3 && verticalMatches.Count < 3)
            {
                BoardDebugger.Log("no match", LogType.FindMatchLineBothSide);
                matchShape = new MatchShape(new ShapeMask(null), ShapeType.I2);
                return false;
            }

            BoardDebugger.Log("match !", LogType.FindMatchLineBothSide);
            if (horizontalMatches.Count >= verticalMatches.Count)
                matchShape = new MatchShape(new ShapeMask(horizontalMatches.ToArray()), ShapeType.I2);
            else
                matchShape = new MatchShape(new ShapeMask(verticalMatches.ToArray()), ShapeType.I2);
            return true;
        }

        private void FindMatchToward(BlockData blockData, BoardVec2 dir, ref List<BoardVec2> matches)
        {
            for (int i = 1; i <= 2; i++)
            {
                var nextPos = blockData.array2dIdx + dir * i;
                BoardDebugger.Log(nextPos, LogType.FindMatchLineBothSide);
                if (IsOutOfBound(nextPos) ||
                    _boardData.GetBlockStateAt(nextPos) == BlockState.Disable ||
                    _boardData.GetCurBlockTypeAt(nextPos) != blockData.currentType)
                    return;
                matches.Add(nextPos);
            }
        }
        
        /*
         * O O
         * O X O
         */
        private bool FindMatchCube(BlockData blockData, out MatchShape matchShape)
        {
            foreach (var shapeMask in MatchShape.GetShapeMasksCube())
            {
                BoardDebugger.Log(blockData.array2dIdx, LogType.FindMatchCube);
                BoardDebugger.Indent(LogType.FindMatchCube);
                bool isMatch = true;
                foreach (var nextCoord in shapeMask.coord)
                {
                    var nextPos = blockData.array2dIdx + nextCoord;
                    BoardDebugger.Log(nextPos, LogType.FindMatchCube);
                    
                    if (!IsOutOfBound(nextPos) && 
                        _boardData.GetBlockStateAt(nextPos) != BlockState.Disable &&
                        _boardData.GetCurBlockTypeAt(nextPos) == blockData.currentType)
                        continue;
                    
                    isMatch = false;
                    break;
                }
                BoardDebugger.Unindent(LogType.FindMatchCube);
                
                if (!isMatch)
                    continue;
                
                BoardDebugger.Log("match !", LogType.FindMatchCube);
                var coord = GetAppliedShapeMask(blockData, shapeMask);
                matchShape = new MatchShape(new ShapeMask(coord), ShapeType.Cube);
                return true;
            }
            
            BoardDebugger.Log("no match", LogType.FindMatchCube);
            matchShape = new MatchShape(new ShapeMask(null), ShapeType.Cube);
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
                    _boardData.GetBlockStateAt(nextPos) != BlockState.Enable ||
                    _boardData.GetCurBlockTypeAt(nextPos) != blockData.currentType ||
                    _visited.Contains(_boardData.GetBlockDataAt(nextPos)))
                    continue;
                TraverseDfs(_boardData.GetBlockDataAt(nextPos));
            }
        }
        
    }
}
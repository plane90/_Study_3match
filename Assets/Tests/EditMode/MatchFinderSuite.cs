using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Board;
using Board.Model;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class MatchFinderSuite
    {
        private MatchFinder _matchFinder;
        private Stopwatch _sw = new Stopwatch();

        [Test(Description = "2x2 매치와 연결된 컴포넌트를 찾을 수 있다.")]
        public void GetConnectedComponentsCube()
        {
            _matchFinder = new MatchFinder(new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Blue, BlockType.Green },
                { BlockType.Blue, BlockType.Red, BlockType.Red },
                { BlockType.Green, BlockType.Red, BlockType.Red },
            }));

            _sw.Restart();
            Assert.IsTrue(_matchFinder.GetMatchedBlocksData().Count(x => x.currentType == BlockType.Red) == 4);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");

            _matchFinder = new MatchFinder(new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Blue, BlockType.Green, BlockType.Blue, BlockType.Blue, },
                { BlockType.Blue, BlockType.Red, BlockType.Red, BlockType.Blue, BlockType.Blue, },
                { BlockType.Green, BlockType.Red, BlockType.Red, BlockType.Green, BlockType.Green },
            }));

            _sw.Restart();
            var matchRed = _matchFinder.GetMatchedBlocksData().Where(x => x.currentType == BlockType.Red);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");
            Assert.IsTrue(matchRed.Count() == 4);
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 1)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 2)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(2, 1)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(2, 2)));

            _sw.Restart();
            var matchBlue = _matchFinder.GetMatchedBlocksData().Where(x => x.currentType == BlockType.Blue);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");
            Assert.IsTrue(matchBlue.Count() == 4);
            Assert.IsTrue(matchBlue.Any(x => x.array2dIdx == new BoardVec2(0, 3)));
            Assert.IsTrue(matchBlue.Any(x => x.array2dIdx == new BoardVec2(0, 4)));
            Assert.IsTrue(matchBlue.Any(x => x.array2dIdx == new BoardVec2(1, 3)));
            Assert.IsTrue(matchBlue.Any(x => x.array2dIdx == new BoardVec2(1, 4)));

            _sw.Stop();
        }

        [Test(Description = "1x3 매치와 연결된 컴포넌트를 찾을 수 있다.")]
        public void GetConnectedComponentsLine()
        {
            _matchFinder = new MatchFinder(new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Blue, BlockType.Red },
                { BlockType.Blue, BlockType.Blue, BlockType.Red },
                { BlockType.Green, BlockType.Green, BlockType.Red },
            }));

            _sw.Restart();
            Assert.IsTrue(_matchFinder.GetMatchedBlocksData().Count(x => x.currentType == BlockType.Red) == 3);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");

            _matchFinder = new MatchFinder(new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Red, BlockType.Green, BlockType.Blue, BlockType.Blue, },
                { BlockType.Red, BlockType.Red, BlockType.Red, BlockType.Red, BlockType.Red, },
                { BlockType.Green, BlockType.Red, BlockType.Blue, BlockType.Green, BlockType.Green },
            }));

            _sw.Restart();
            var matchRed = _matchFinder.GetMatchedBlocksData().Where(x => x.currentType == BlockType.Red);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");
            Assert.IsTrue(matchRed.Count() == 7);
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(0, 1)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 0)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 1)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 2)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 3)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 4)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(2, 1)));

            _sw.Stop();
        }
        
        [Test(Description = "L자형 블록을 찾아낸다.")]
        public void FindMatchL()
        {
            var boardData = new BoardData(new[,]
            {
                { BlockType.Red, BlockType.Green, BlockType.Blue },
                { BlockType.Red, BlockType.Green, BlockType.Blue },
                { BlockType.Red, BlockType.Red, BlockType.Red },
            });
            _matchFinder = new MatchFinder(boardData);

            _sw.Restart();
            _matchFinder.IsMatchAt(boardData.GetBlockDataAt(2, 0), out var matches);
            UnityEngine.Debug.Log($"IsMatchAt Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");
            Assert.IsTrue(matches.Count == 5);
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(0, 0)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(1, 0)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(2, 0)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(2, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(2, 2)));

            _sw.Stop();
        }
        
        [Test(Description = "T자형 블록을 찾아낸다.")]
        public void FindMatchT()
        {
            var boardData = new BoardData(new[,]
            {
                { BlockType.Red, BlockType.Red, BlockType.Red },
                { BlockType.Blue, BlockType.Red, BlockType.Green },
                { BlockType.Blue, BlockType.Red, BlockType.Blue },
            });
            _matchFinder = new MatchFinder(boardData);

            _sw.Restart();
            _matchFinder.IsMatchAt(boardData.GetBlockDataAt(0, 1), out var matches);
            UnityEngine.Debug.Log($"IsMatchAt Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");
            Assert.IsTrue(matches.Count == 5);
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(0, 0)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(0, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(0, 2)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(1, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(2, 1)));

            _sw.Stop();
        }
        
        [Test(Description = "Cube형 블록을 찾아낸다.")]
        public void FindMatchCube()
        {
            var boardData = new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Red, BlockType.Red },
                { BlockType.Blue, BlockType.Red, BlockType.Red },
                { BlockType.Blue, BlockType.Green, BlockType.Blue },
            });
            _matchFinder = new MatchFinder(boardData);

            _sw.Restart();
            _matchFinder.IsMatchAt(boardData.GetBlockDataAt(0, 1), out var matches);
            UnityEngine.Debug.Log($"IsMatchAt Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");
            Assert.IsTrue(matches.Count == 4);
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(0, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(0, 2)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(1, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(1, 2)));

            _sw.Stop();
        }
        
        [Test(Description = "I자형(양옆 체크) 블록을 찾아낸다.")]
        public void FindMatchBothSide()
        {
            var boardData = new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Red, BlockType.Red },
                { BlockType.Blue, BlockType.Red, BlockType.Green },
                { BlockType.Blue, BlockType.Red, BlockType.Blue },
            });
            _matchFinder = new MatchFinder(boardData);

            HashSet<BlockData> matches;
            
            _sw.Restart();
            _matchFinder.IsMatchAt(boardData.GetBlockDataAt(1, 1), out matches);
            UnityEngine.Debug.Log($"IsMatchAt Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");
            Assert.IsTrue(matches.Count == 3);
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(0, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(1, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(2, 1)));
            
            boardData = new BoardData(new[,]
            {
                { BlockType.Blue, BlockType.Green, BlockType.Blue, },
                { BlockType.Red, BlockType.Blue, BlockType.Blue, },
                { BlockType.Green, BlockType.Yellow, BlockType.Green, },
            });
            _matchFinder = new MatchFinder(boardData);

            _sw.Restart();
            Assert.IsFalse(_matchFinder.IsMatchAt(boardData.GetBlockDataAt(1, 1), out matches));
            UnityEngine.Debug.Log($"IsMatchAt Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");

            _sw.Stop();
        }
        
        [Test(Description = "I자형(일방향 체크) 블록을 찾아낸다.")]
        public void FindMatchOneSide()
        {
            var boardData = new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Red, BlockType.Red },
                { BlockType.Blue, BlockType.Red, BlockType.Green },
                { BlockType.Blue, BlockType.Red, BlockType.Blue },
            });
            _matchFinder = new MatchFinder(boardData);

            _sw.Restart();
            _matchFinder.IsMatchAt(boardData.GetBlockDataAt(0, 1), out var matches);
            UnityEngine.Debug.Log($"IsMatchAt Done: {_sw.Elapsed.TotalMilliseconds.ToString()}ms");
            Assert.IsTrue(matches.Count == 3);
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(0, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(1, 1)));
            Assert.IsTrue(matches.Contains(boardData.GetBlockDataAt(2, 1)));

            _sw.Stop();
        }
    }
}
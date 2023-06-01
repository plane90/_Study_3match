using System.Diagnostics;
using System.Linq;
using Board.Model;
using NUnit.Framework;

namespace Board.TestEditMode
{
    public class MatchFinderSuite
    {
        private MatchFinder _matchFinder;
        private Stopwatch _sw = new Stopwatch();
    
        [Test(Description = "2x2 매치와 연결된 컴포넌트를 찾을 수 있다.")]
        public void FindMatchCube()
        {
            _matchFinder = new MatchFinder(new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Blue, BlockType.Green },
                { BlockType.Blue, BlockType.Red, BlockType.Red },
                { BlockType.Green, BlockType.Red, BlockType.Red },
            }));

            _sw.Restart();
            Assert.IsTrue(_matchFinder.GetMatchedBlocksData().Count(x => x.currentType == BlockType.Red) == 4);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}");
        
            _matchFinder = new MatchFinder( new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Blue, BlockType.Green, BlockType.Blue, BlockType.Blue, },
                { BlockType.Blue, BlockType.Red, BlockType.Red, BlockType.Blue, BlockType.Blue, },
                { BlockType.Green, BlockType.Red, BlockType.Red, BlockType.Green, BlockType.Green },
            }));
        
            _sw.Restart();
            var matchRed = _matchFinder.GetMatchedBlocksData().Where(x => x.currentType == BlockType.Red);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}");
            Assert.IsTrue(matchRed.Count() == 4);
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 1)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(1, 2)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(2, 1)));
            Assert.IsTrue(matchRed.Any(x => x.array2dIdx == new BoardVec2(2, 2)));

            _sw.Restart();
            var matchBlue = _matchFinder.GetMatchedBlocksData().Where(x => x.currentType == BlockType.Blue);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}");
            Assert.IsTrue(matchBlue.Count() == 4);
            Assert.IsTrue(matchBlue.Any(x => x.array2dIdx == new BoardVec2(0, 3)));
            Assert.IsTrue(matchBlue.Any(x => x.array2dIdx == new BoardVec2(0, 4)));
            Assert.IsTrue(matchBlue.Any(x => x.array2dIdx == new BoardVec2(1, 3)));
            Assert.IsTrue(matchBlue.Any(x => x.array2dIdx == new BoardVec2(1, 4)));
        
            _sw.Stop();
        }
    
        [Test(Description = "1x3 매치와 연결된 컴포넌트를 찾을 수 있다.")]
        public void FindMatchLine()
        {
            _matchFinder = new MatchFinder(new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Blue, BlockType.Red },
                { BlockType.Blue, BlockType.Blue, BlockType.Red },
                { BlockType.Green, BlockType.Green, BlockType.Red },
            }));
        
            _sw.Restart();
            Assert.IsTrue(_matchFinder.GetMatchedBlocksData().Count(x => x.currentType == BlockType.Red) == 3);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}");
        
            _matchFinder = new MatchFinder(new BoardData(new[,]
            {
                { BlockType.Green, BlockType.Red, BlockType.Green, BlockType.Blue, BlockType.Blue, },
                { BlockType.Red, BlockType.Red, BlockType.Red, BlockType.Red, BlockType.Red, },
                { BlockType.Green, BlockType.Red, BlockType.Blue, BlockType.Green, BlockType.Green },
            }));

            _sw.Restart();
            var matchRed = _matchFinder.GetMatchedBlocksData().Where(x => x.currentType == BlockType.Red);
            UnityEngine.Debug.Log($"GetMatchedBlocksData Done: {_sw.Elapsed.TotalMilliseconds.ToString()}");
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
    }
}
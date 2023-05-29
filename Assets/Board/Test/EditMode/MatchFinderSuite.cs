using System.Collections;
using System.Linq;
using Board.Model;
using Board;
using NUnit.Framework;
using UnityEngine.TestTools;

public class MatchFinderSuite
{
    [Test(Description = "2x2 매치와 연결된 컴포넌트를 찾을 수 있다.")]
    public void FindMatchCube()
    {
        BlockType[,] types;
        BoardData boardData;
        types = new[,]
        {
            { BlockType.Green, BlockType.Blue, BlockType.Green },
            { BlockType.Blue, BlockType.Red, BlockType.Red },
            { BlockType.Green, BlockType.Red, BlockType.Red },
        };
        boardData = CreateBoardData(types);

        Assert.IsTrue(MatchFinder.GetMatchedBlocksData(boardData).Count(x => x.blockType == BlockType.Red) == 4);
        
        types = new[,]
        {
            { BlockType.Green, BlockType.Blue, BlockType.Green, BlockType.Blue, BlockType.Blue, },
            { BlockType.Blue, BlockType.Red, BlockType.Red, BlockType.Blue, BlockType.Blue, },
            { BlockType.Green, BlockType.Red, BlockType.Red, BlockType.Green, BlockType.Green },
        };
        boardData = CreateBoardData(types);
        
        var matchRed = MatchFinder.GetMatchedBlocksData(boardData).Where(x => x.blockType == BlockType.Red);
        Assert.IsTrue(matchRed.Count() == 4);
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(1, 1)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(1, 2)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(2, 1)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(2, 2)));

        var matchBlue = MatchFinder.GetMatchedBlocksData(boardData).Where(x => x.blockType == BlockType.Blue);
        Assert.IsTrue(matchBlue.Count() == 4);
        Assert.IsTrue(matchBlue.Any(x => x.idxArray2D == new Vec2(0, 3)));
        Assert.IsTrue(matchBlue.Any(x => x.idxArray2D == new Vec2(0, 4)));
        Assert.IsTrue(matchBlue.Any(x => x.idxArray2D == new Vec2(1, 3)));
        Assert.IsTrue(matchBlue.Any(x => x.idxArray2D == new Vec2(1, 4)));
    }
    
    [Test(Description = "1x3 매치와 연결된 컴포넌트를 찾을 수 있다.")]
    public void FindMatchLine()
    {
        BlockType[,] types;
        BoardData boardData;
        types = new[,]
        {
            { BlockType.Green, BlockType.Blue, BlockType.Red },
            { BlockType.Blue, BlockType.Blue, BlockType.Red },
            { BlockType.Green, BlockType.Green, BlockType.Red },
        };
        boardData = CreateBoardData(types);

        Assert.IsTrue(MatchFinder.GetMatchedBlocksData(boardData).Count(x => x.blockType == BlockType.Red) == 3);
        
        types = new[,]
        {
            { BlockType.Green, BlockType.Red, BlockType.Green, BlockType.Blue, BlockType.Blue, },
            { BlockType.Red, BlockType.Red, BlockType.Red, BlockType.Red, BlockType.Red, },
            { BlockType.Green, BlockType.Red, BlockType.Blue, BlockType.Green, BlockType.Green },
        };
        boardData = CreateBoardData(types);

        var matchRed = MatchFinder.GetMatchedBlocksData(boardData).Where(x => x.blockType == BlockType.Red);
        Assert.IsTrue(matchRed.Count() == 7);
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(0, 1)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(1, 0)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(1, 1)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(1, 2)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(1, 3)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(1, 4)));
        Assert.IsTrue(matchRed.Any(x => x.idxArray2D == new Vec2(2, 1)));
    }

    private BoardData CreateBoardData(BlockType[,] types) => new(types.GetLength(0), types.GetLength(1), types);
}
namespace Board.Model
{
    public enum BlockType
    {
        Random, None, Red, Green, Blue, Yellow, End,
    }
    
    [System.Serializable]
    public class BlockData
    {
        public BlockType blockType;
        public Vec2 idxArray2D;

        public BlockData(Vec2 idxArray2D, BlockType blockType = BlockType.Random)
        {
            this.idxArray2D = idxArray2D;
            this.blockType = blockType;
        }

        public static BlockData[,] CreateBlockDataArray2D(int rows, int cols, BlockType[,] blockTypes)
        {
            var blockDataArray2D = new BlockData[rows, cols];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    var blockData = new BlockData(new Vec2(y, x), blockTypes[y, x]);
                    blockDataArray2D[y, x] = blockData;
                }
            }
            return blockDataArray2D;
        }
    }
}
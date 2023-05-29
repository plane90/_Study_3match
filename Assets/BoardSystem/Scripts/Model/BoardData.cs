namespace Board.Model
{
    [System.Serializable]
    public class BoardData
    {
        public BlockData[,] BlockDataArray2D { get; }
        public int Rows { get; }
        public int Cols { get; }

        public BoardData(BoardData other)
        {
            Rows = other.Rows;
            Cols = other.Cols;

            BlockDataArray2D = other.BlockDataArray2D.Clone() as BlockData[,];
        }
        
        public BoardData(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;

            BlockDataArray2D = new BlockData[rows, cols];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    BlockDataArray2D[y, x] = new BlockData(new Vec2(y, x));
                }
            }
        }
        
        public BoardData(int rows, int cols, BlockType[,] blockTypes)
        {
            Rows = rows;
            Cols = cols;

            BlockDataArray2D = new BlockData[rows, cols];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    BlockDataArray2D[y, x] = new BlockData(new Vec2(y, x), blockTypes[y, x]);
                }
            }
        }
        
        public BoardData(int rows, int cols, BlockData[,] blockDataArray2D)
        {
            Rows = rows;
            Cols = cols;

            BlockDataArray2D = blockDataArray2D.Clone() as BlockData[,];
        }

        public BlockType GetBlockTypeAt(Vec2 idxArray2D) => BlockDataArray2D[idxArray2D.Y, idxArray2D.X].blockType;
        
        public BlockData GetBlockDataAt(Vec2 idxArray2D) => BlockDataArray2D[idxArray2D.Y, idxArray2D.X];
    }
}
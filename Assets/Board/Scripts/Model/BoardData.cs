namespace Board.Model
{
    [System.Serializable]
    public class BoardData
    {
        public BlockData[,] BlockDataArray2D { get; }
        public int Rows { get; }
        public int Cols { get; }
        
        public BoardData(int rows, int cols) : this(rows, cols, new BlockType[rows, cols]) { }
        
        public BoardData(int rows, int cols, BlockType[,] blockTypes)
        {
            Rows = rows;
            Cols = cols;

            BlockDataArray2D = BlockData.CreateBlockDataArray2D(rows, cols, blockTypes);
        }
        
        public BoardData(int rows, int cols, BlockData[,] blockDataArray2D)
        {
            Rows = rows;
            Cols = cols;

            BlockDataArray2D = blockDataArray2D.Clone() as BlockData[,];
        }

        public BoardData(BoardData other) : this(other.Rows, other.Cols, other.BlockDataArray2D) { }

        public BlockType GetBlockTypeAt(Vec2 idxArray2D) => BlockDataArray2D[idxArray2D.Y, idxArray2D.X].blockType;
        
        public BlockData GetBlockDataAt(Vec2 idxArray2D) => BlockDataArray2D[idxArray2D.Y, idxArray2D.X];
    }
}
namespace Board.Model
{
    [System.Serializable]
    public class BoardData
    {
        public BlockData[,] BlockDataArray2D { get; }
        public int Rows { get; }
        public int Cols { get; }
        
        public BoardData(int rows, int cols) : this(new BlockType[rows, cols]) { }
        
        public BoardData(BlockType[,] blockTypes)
        {
            Rows = blockTypes.GetLength(0);
            Cols = blockTypes.GetLength(1);

            BlockDataArray2D = BlockData.CreateBlockDataArray2D(Rows, Cols, blockTypes);
        }
        
        public BoardData(int rows, int cols, BlockData[,] blockDataArray2D)
        {
            Rows = rows;
            Cols = cols;

            BlockDataArray2D = blockDataArray2D.Clone() as BlockData[,];
        }

        public BoardData(BoardData other) : this(other.Rows, other.Cols, other.BlockDataArray2D) { }

        public BlockType GetCurBlockTypeAt(BoardVec2 array2dIdx) => BlockDataArray2D[array2dIdx.Y, array2dIdx.X].currentType;
        
        public BlockData GetBlockDataAt(BoardVec2 array2dIdx) => BlockDataArray2D[array2dIdx.Y, array2dIdx.X];
        
        public BlockData GetBlockDataAt(int y, int x) => BlockDataArray2D[y, x];

        public void SetBlockDataAt(BoardVec2 array2dIdx, BlockData blockData) => BlockDataArray2D[array2dIdx.Y, array2dIdx.X] = blockData;

        public BlockState GetBlockStateAt(BoardVec2 array2dIdx) => BlockDataArray2D[array2dIdx.Y, array2dIdx.X].state;
    }
}
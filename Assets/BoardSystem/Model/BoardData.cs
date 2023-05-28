namespace BlockSystem.Model
{
    [System.Serializable]
    public class BoardData
    {
        public BlockData[,] BlockDataArray2D { get; }
        public int Rows { get; }
        public int Cols { get; }

        public BoardData(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;

            BlockDataArray2D = new BlockData[rows, cols];
        }
    }
}
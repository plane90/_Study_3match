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

        public BlockData(Vec2 idxArray2D)
        {
            this.idxArray2D = idxArray2D;
        }
        
        public BlockData(Vec2 idxArray2D, BlockType blockType)
        {
            this.idxArray2D = idxArray2D;
            this.blockType = blockType;
        }
    }
}
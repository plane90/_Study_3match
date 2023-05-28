namespace BlockSystem.Model
{
    public enum BlockType
    {
        Random, None, Red, Green, Blue, Yellow, End,
    }

    [System.Serializable]
    public struct BlockData
    {
        public BlockType blockType;
    }
}
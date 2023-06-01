using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board.Model
{
    public enum BlockType
    {
        Random, None, Red, Green, Blue, Yellow, End,
    }
    
    public enum BlockState
    {
        Disable, Enable, Moving, Ready,
    }
    
    [System.Serializable]
    public class BlockData
    {
        public BlockType currentType;
        private BlockType _initType;
        public BoardVec2 array2dIdx;
        public BlockState state;

        public BlockData(BoardVec2 array2dIdx, BlockType initType = BlockType.Random)
        {
            this.array2dIdx = array2dIdx;
            _initType = initType;
            SetCurBlockType(this);
        }

        public static BlockData[,] CreateBlockDataArray2D(int rows, int cols, BlockType[,] blockTypes)
        {
            var blockDataArray2D = new BlockData[rows, cols];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    var blockData = new BlockData(new BoardVec2(y, x), blockTypes[y, x]);
                    blockDataArray2D[y, x] = blockData;
                }
            }
            return blockDataArray2D;
        }

        public static void PopulateBlockType(BlockData[,] blocksData)
        {
            foreach (var blockData in blocksData)
            {
                SetCurBlockType(blockData);
            }
        }

        public static void PopulateBlockType(IEnumerable<BlockData> blocksData)
        {
            foreach (var blockData in blocksData)
            {
                SetCurBlockType(blockData);
            }
        }

        private static void SetCurBlockType(BlockData blockData)
        {
            if (blockData._initType == BlockType.Random)
            {
                blockData.currentType = (BlockType)Random.Range((int)BlockType.Red, (int)BlockType.End);
            }
            else
            {
                blockData.currentType = blockData._initType;
            }
        }
    }
}
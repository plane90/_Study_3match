using System.Collections.Generic;
using Board.Model;
using Board.View;

namespace Board
{
    public static class Shuffle
    {
        private static readonly System.Random Random = new();
        public static void Execute(BlockData[,] blocksData, Block[,] blocks)
        {
            // 2차원 배열을 1차원으로.
            var flatArrBlocksData = new List<BlockData>();
            var flatArrBlocks = new List<Block>();
            
            int rows = blocksData.GetLength(0);
            int cols = blocksData.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    flatArrBlocksData.Add(blocksData[i, j]);
                    flatArrBlocks.Add(blocks[i, j]);
                }
            }

            // Fisher-Yates 알고리즘을 사용하여 1차원 리스트를 셔플.
            int n = flatArrBlocksData.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                (flatArrBlocksData[k].array2dIdx, flatArrBlocksData[n].array2dIdx) = (flatArrBlocksData[n].array2dIdx, flatArrBlocksData[k].array2dIdx);
                (flatArrBlocksData[k], flatArrBlocksData[n]) = (flatArrBlocksData[n], flatArrBlocksData[k]);
                
                (flatArrBlocks[k], flatArrBlocks[n]) = (flatArrBlocks[n], flatArrBlocks[k]);
            }

            // 섞인 1차원 리스트를 다시 2차원 배열로 변환.
            int index = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    blocksData[i, j] = flatArrBlocksData[index];
                    blocks[i, j] = flatArrBlocks[index];
                    index++;
                }
            }
        }
    }
}
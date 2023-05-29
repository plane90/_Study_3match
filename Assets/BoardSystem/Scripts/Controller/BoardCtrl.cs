using BlockSystem.Model;
using BoardSystem.View;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoardSystem.Presenter
{
    public class BoardCtrl: MonoBehaviour
    {
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private RectTransform _boardObj;

        public int rows, cols;

        private BoardData _boardData;
        private Block[,] _blocks;

        [MyBox.ButtonMethod()]
        public void InitBoardData()
        {
            _boardData = new BoardData(rows, cols);

            foreach (var blockData in _boardData.BlockDataArray2D)
            {
                if (blockData.blockType == BlockType.Random)
                {
                    blockData.blockType = (BlockType)Random.Range((int)BlockType.Red, (int)BlockType.End);
                }
            }
            
            ValidateBoard();
        }
        
        private void ValidateBoard()
        {
            var matchedBlocksData = MatchFinder.GetMatchedBlocksData(_boardData);
            while (matchedBlocksData.Count > 0)
            {
                foreach (var blockData in matchedBlocksData)
                {
                    blockData.blockType = (BlockType)Random.Range((int)BlockType.Red, (int)BlockType.End);
                }
                matchedBlocksData = MatchFinder.GetMatchedBlocksData(_boardData);
            }
        }

        [MyBox.ButtonMethod()]
        public void CreateBoardAndFillBlock()
        {
            // Init Board RectTransform
            var canvasWidth = _boardObj.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
            var paddingWidth = canvasWidth * 0.05f;
            var blockRectSize = (canvasWidth - paddingWidth) / 9f;
            var boardRectWidth = paddingWidth + blockRectSize * _boardData.Cols;
            var boardRectHeight = paddingWidth + blockRectSize * _boardData.Rows;
            _boardObj.sizeDelta = new Vector2(boardRectWidth, boardRectHeight);

            // Fill Blocks
            for (int y = 0; y < _boardData.Rows; y++)
            {
                for (int x = 0; x < _boardData.Cols; x++)
                {
                    var block = Instantiate(_blockPrefab, _boardObj).GetComponent<Block>();
                    var posX = paddingWidth * 0.5f + blockRectSize * x;
                    var posY = paddingWidth * 0.5f + blockRectSize * y;
                    var rect = block.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(posX, -posY);
                    rect.sizeDelta = Vector2.one * blockRectSize;

                    block.Init(_boardData.BlockDataArray2D[y, x]);
                }
            }
        }

        [MyBox.ButtonMethod()]
        public void Reset()
        {
            for (var i = _boardObj.childCount - 1; i >= 0 ; i--)
            {
                DestroyImmediate(_boardObj.GetChild(i).gameObject);
            }
        }
    }
}
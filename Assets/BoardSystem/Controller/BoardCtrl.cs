using BlockSystem.Model;
using BoardSystem.View;
using UnityEngine;

namespace BoardSystem.Presenter
{
    public class BoardCtrl: MonoBehaviour
    {
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private RectTransform _board;

        public int rows, cols;

        private BoardData _boardData;

        [MyBox.ButtonMethod()]
        public void InitBoardData()
        {
            _boardData = new BoardData(rows, cols);
        }

        [MyBox.ButtonMethod()]
        public void CreateBoard()
        {
            // Init Board RectTransform
            var canvasWidth = _board.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
            var paddingWidth =  canvasWidth * 0.05f;
            var blockSize = (canvasWidth - paddingWidth) / 9f;
            var boardWidth = paddingWidth + blockSize * _boardData.Cols;
            var boardHeight = paddingWidth + blockSize * _boardData.Rows;
            _board.sizeDelta = new Vector2(boardWidth, boardHeight);
            
            // Fill Blocks
            var blockDataArray2D = _boardData.BlockDataArray2D;
            for (int y = 0; y < blockDataArray2D.GetLength(0); y++)
            {
                for (int x = 0; x < blockDataArray2D.GetLength(1); x++)
                {
                    var block = Instantiate(_blockPrefab, _board).GetComponent<Block>();
                    var posX = paddingWidth * 0.5f + blockSize * x; 
                    var posY = paddingWidth * 0.5f + blockSize * y;
                    block.InitBlock(new Vector2(posX, -posY), blockSize, blockDataArray2D[y,x]);
                }
            }
        }
    }
}
using UnityEngine;

namespace Board
{
    public readonly struct BoardRectInfo
    {
        private readonly RectTransform _coordMarker;
        private readonly float _paddingWidth;
        private readonly Vector3 _topLeftWorldPos;
        private readonly float _deltaWorldPosX;
        private readonly float _deltaWorldPosY;

        public readonly float blockRectSize;
        
        public BoardRectInfo(int rows, int cols, RectTransform boardRectTransform)
        {
            var canvasWidth = boardRectTransform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
            _paddingWidth = canvasWidth * 0.05f;
            blockRectSize = (canvasWidth - _paddingWidth) / 9f;
            var boardRectWidth = _paddingWidth + blockRectSize * cols;
            var boardRectHeight = _paddingWidth + blockRectSize * rows;
            boardRectTransform.sizeDelta = new Vector2(boardRectWidth, boardRectHeight);
            
            _coordMarker = new GameObject("marker", typeof(RectTransform)).GetComponent<RectTransform>();
            _topLeftWorldPos = Vector3.zero;
            _deltaWorldPosX = 0;
            _deltaWorldPosY = 0;
            InitCoordMarker(boardRectTransform);
            
            _coordMarker.anchoredPosition = GetBlockAnchoredPosAt(new BoardVec2(0, 0));
            _topLeftWorldPos = _coordMarker.position;
            _coordMarker.anchoredPosition = GetBlockAnchoredPosAt(new BoardVec2(1, 1));
            _deltaWorldPosX = Mathf.Abs(_topLeftWorldPos.x - _coordMarker.position.x);
            _deltaWorldPosY = Mathf.Abs(_topLeftWorldPos.y - _coordMarker.position.y);
            _coordMarker.gameObject.SetActive(false);
        }

        private void InitCoordMarker(RectTransform parent)
        {
            _coordMarker.SetParent(parent);
            _coordMarker.anchorMin = _coordMarker.anchorMax = _coordMarker.pivot = Vector2.up;
            _coordMarker.localPosition = Vector3.zero;
            _coordMarker.sizeDelta = Vector2.one;
        }

        public Vector2 GetBlockAnchoredPosAt(BoardVec2 array2dIdx)
        {
            var posX = _paddingWidth * 0.5f + blockRectSize * array2dIdx.X;
            var posY = _paddingWidth * 0.5f + blockRectSize * array2dIdx.Y;
            return new Vector2(posX, -posY);
        }

        public Vector3 GetBlockWorldPosAt(BoardVec2 array2dIdx)
        {
            var posX = _topLeftWorldPos.x + _deltaWorldPosX * array2dIdx.X;
            var posY = _topLeftWorldPos.y - _deltaWorldPosY * array2dIdx.Y;
            return new Vector3(posX, posY, _topLeftWorldPos.z);
        }
    }
}
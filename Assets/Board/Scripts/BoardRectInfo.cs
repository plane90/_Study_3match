using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public readonly struct BoardRectInfo
    {
        private readonly RectTransform _coordMarker;
        private readonly int _rows;
        private readonly int _cols;
        private readonly float _paddingWidth;
        private readonly Dictionary<BoardVec2, Vector3> _worldPosMap;

        public readonly float blockRectSize;
        
        public BoardRectInfo(int rows, int cols, RectTransform boardRectTransform)
        {
            var canvasWidth = boardRectTransform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
            _paddingWidth = canvasWidth * 0.05f;
            blockRectSize = (canvasWidth - _paddingWidth) / 9f;
            var boardRectWidth = _paddingWidth + blockRectSize * cols;
            var boardRectHeight = _paddingWidth + blockRectSize * rows;
            boardRectTransform.sizeDelta = new Vector2(boardRectWidth, boardRectHeight);

            _rows = rows;
            _cols = cols;

            _coordMarker = new GameObject("marker", typeof(RectTransform)).GetComponent<RectTransform>();
            _worldPosMap = new Dictionary<BoardVec2, Vector3>();
            InitCoordMarker(boardRectTransform);
            InitWorldPosMap();
        }

        private void InitCoordMarker(RectTransform parent)
        {
            _coordMarker.SetParent(parent);
            _coordMarker.anchorMin = _coordMarker.anchorMax = _coordMarker.pivot = Vector2.up;
            _coordMarker.localPosition = Vector3.zero;
            _coordMarker.sizeDelta = Vector2.one;
        }

        private void InitWorldPosMap()
        {
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _cols; x++)
                {
                    var idx = new BoardVec2(y, x);
                    _coordMarker.anchoredPosition = GetBlockAnchoredPosAt(idx);
                    _worldPosMap[idx] = _coordMarker.position;
                }
            }
            _coordMarker.gameObject.SetActive(false);
        }

        public Vector2 GetBlockAnchoredPosAt(BoardVec2 array2dIdx)
        {
            var posX = _paddingWidth * 0.5f + blockRectSize * array2dIdx.X;
            var posY = _paddingWidth * 0.5f + blockRectSize * array2dIdx.Y;
            return new Vector2(posX, -posY);
        }

        public Vector3 GetBlockWorldPosAt(BoardVec2 array2dIdx) => _worldPosMap[array2dIdx];
    }
}
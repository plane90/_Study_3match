using System;
using System.Collections.Generic;
using Board.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Board.View
{
    public class Block: MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rectTransform;
        private Vector2 _dragStartPos;
        private bool _isDragged;

        public Action<Block, BoardVec2> draggedBlock;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _dragStartPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragged)
                return;
            
            var dragDir = eventData.position - _dragStartPos;
            if (dragDir.magnitude < _rectTransform.rect.width / 2f)
                return;
            if (CheckDirection(dragDir, out var toDir))
            {
                draggedBlock?.Invoke(this, toDir);
                _isDragged = true;
            }
        }

        private bool CheckDirection(Vector2 dragDir, out BoardVec2 toDir)
        {
            toDir = BoardVec2.zero;
            var angle = Mathf.Atan2(dragDir.y, dragDir.x) * Mathf.Rad2Deg;
            angle += (angle < 0) ? 360f : 0f;
            toDir = angle switch
            {
                <= 20f or >= 340f => BoardVec2.right,
                >= 70f and <= 110f => BoardVec2.up,
                >= 160f and <= 200f => BoardVec2.left,
                >= 250f and <= 290f => BoardVec2.down,
                _ => toDir
            };
            return toDir != BoardVec2.zero;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragged = false;
        }
    }
}
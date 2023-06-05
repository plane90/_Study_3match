using System;
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
        private Vector3 _anchorPosDest;
        private bool _isDrop = false;
        private readonly float _initVelocity = -100f;
        private float _velocity;
        private readonly float _gravity = -50f;

        public Action<Block, BoardVec2> draggedBlock;
        public Action<Block> droppedBlock;

        [MyBox.ButtonMethod()]
        public void Test()
        {
            _isDrop = !_isDrop;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void HideAt(Vector2 pos)
        {
            _rectTransform.anchoredPosition = pos;
            this.enabled = false;
        }

        public void Show()
        {
            _rectTransform.localScale = Vector3.one;
            this.enabled = true;
        }

        public void SetLooks(Color color)
        {
            GetComponent<Image>().color = color;
        }

        public void Drop(Vector3 anchorPosDest)
        {
            _anchorPosDest = anchorPosDest;
            _isDrop = true;
        }

        public void Stop()
        {
            _isDrop = false;
            _velocity = _initVelocity;
        }

        private void Update()
        {
            if (!_isDrop)
                return;

            ApplyGravity();
        }

        private void ApplyGravity()
        {
            _velocity += _gravity * Time.fixedDeltaTime;

            _rectTransform.anchoredPosition += new Vector2(0f, _velocity * Time.fixedDeltaTime);

            if (Mathf.Abs(_anchorPosDest.y - _rectTransform.anchoredPosition.y) < 10.0f)
            {
                _isDrop = false;
                _rectTransform.anchoredPosition = _anchorPosDest;
                droppedBlock?.Invoke(this);
            }
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
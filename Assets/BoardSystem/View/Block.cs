using System.Collections.Generic;
using BlockSystem.Model;
using UnityEngine;
using UnityEngine.UI;

namespace BoardSystem.View
{
    public class Block: MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;

        private readonly Dictionary<BlockType, Color> _typeColorMap = new()
        {
            { BlockType.Red, Color.red },
            { BlockType.Green, Color.green },
            { BlockType.Blue, Color.blue },
            { BlockType.Yellow, Color.yellow },
        };

        private float _blockSize;
        
        public Vector2 PosOnRect { get; set; }
        public BlockData BlockData { get; set; }
        
        
        public void InitBlock(Vector2 posOnRect, float blockSize, BlockData blockData)
        {
            this.PosOnRect = posOnRect;
            this.BlockData = blockData;
            _blockSize = blockSize;
            
            // init Rect Pos & RectSize
            _rectTransform.anchoredPosition = new Vector2(PosOnRect.x, PosOnRect.y);
            _rectTransform.sizeDelta = Vector2.one * _blockSize;
            
            // init type
            var type = BlockData.blockType;
            if (type == BlockType.Random)
            {
                type = (BlockType)Random.Range((int)BlockType.Red, (int)BlockType.End);
            }
            _image.color = _typeColorMap[type];
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}
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
        
        public Vec2 IdxArray2D { get; set; }
        public BlockType BlockType { get; set; }
        
        
        public void Init(BlockData blockData)
        {
            IdxArray2D = blockData.idxArray2D;
            BlockType = blockData.blockType;
            
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            
            // init type
            _image.color = _typeColorMap[blockData.blockType];
        }
    }
}
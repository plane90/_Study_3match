using System;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public enum ShapeType
    {
        L, T, I1, I2, Cube,
    }
    
    public struct ShapeMask
    {
        public BoardVec2[] coord;

        public ShapeMask(BoardVec2[] coord)
        {
            this.coord = coord;
        }
    }
    
    public struct MatchShape
    {
        public int value;
        public ShapeMask shapeMask;
        public ShapeType shapeType;

        private static readonly Dictionary<ShapeType, List<ShapeMask>> ShapeMaskMap = new()
        {
            { ShapeType.L, new List<ShapeMask>()
            {
                new(new []{ BoardVec2.up, BoardVec2.up * 2, BoardVec2.right, BoardVec2.right * 2 }),
                new(new []{ BoardVec2.right, BoardVec2.right * 2, BoardVec2.down, BoardVec2.down * 2 }),
                new(new []{ BoardVec2.down, BoardVec2.down * 2, BoardVec2.left, BoardVec2.left * 2 }),
                new(new []{ BoardVec2.left, BoardVec2.left * 2, BoardVec2.up, BoardVec2.up * 2 }),
            }},
            { ShapeType.T, new List<ShapeMask>()
            {
                new(new []{ BoardVec2.up, BoardVec2.up * 2, BoardVec2.left, BoardVec2.right }),
                new(new []{ BoardVec2.right, BoardVec2.right * 2, BoardVec2.up, BoardVec2.down }),
                new(new[]{ BoardVec2.down, BoardVec2.down * 2, BoardVec2.left, BoardVec2.right }),
                new(new[]{ BoardVec2.left, BoardVec2.left * 2, BoardVec2.up, BoardVec2.down }),
                
                new(new []{ BoardVec2.up, BoardVec2.up * 2, BoardVec2.left, BoardVec2.left * 2, BoardVec2.right, BoardVec2.right * 2 }),
                new(new []{ BoardVec2.right, BoardVec2.right * 2, BoardVec2.up, BoardVec2.up * 2, BoardVec2.down, BoardVec2.down * 2 }),
                new(new[]{ BoardVec2.down, BoardVec2.down * 2, BoardVec2.left, BoardVec2.left * 2, BoardVec2.right, BoardVec2.right * 2 }),
                new(new[]{ BoardVec2.left, BoardVec2.left * 2, BoardVec2.up, BoardVec2.up * 2, BoardVec2.down, BoardVec2.down * 2 }),
            }},
            { ShapeType.Cube, new List<ShapeMask>()
            {
                new(new []{ BoardVec2.up, BoardVec2.up + BoardVec2.right, BoardVec2.right }),
                new(new []{ BoardVec2.right, BoardVec2.right + BoardVec2.down, BoardVec2.down }),
                new(new []{ BoardVec2.down, BoardVec2.down + BoardVec2.left, BoardVec2.left }),
                new(new []{ BoardVec2.left, BoardVec2.left + BoardVec2.up, BoardVec2.up }),
            }},
        };

        public MatchShape(ShapeMask shapeMask, ShapeType shapeType)
        {
            this.shapeMask = shapeMask;
            value = shapeMask.coord?.Length ?? -1;
            this.shapeType = shapeType;
        }

        public static List<ShapeMask> GetShapeMasksL() => ShapeMaskMap[ShapeType.L];
        
        public static List<ShapeMask> GetShapeMasksT() => ShapeMaskMap[ShapeType.T];
        
        public static List<ShapeMask> GetShapeMasksCube() => ShapeMaskMap[ShapeType.Cube];
    }
}
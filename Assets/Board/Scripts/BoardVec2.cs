using System;

namespace Board
{
    [Serializable]
    public struct BoardVec2
    {
        /// row
        public int Y { get; set; }
        /// column
        public int X { get; set; }

        /// (y, x) = (-1, 0)
        public static BoardVec2 up = new(-1, 0);
        /// (y, x) = (0, 1)
        public static BoardVec2 right = new(0, 1);
        /// (y, x) = (1, 0)
        public static BoardVec2 down = new(1, 0);
        /// (y, x) = (0, -1)
        public static BoardVec2 left = new(0, -1);
        /// (y, x) = (0, 0)
        public static BoardVec2 zero = new(0, 0); 

        /// <summary>
        /// TopLeft: [0,0], BottomRight: [rows,cols]
        /// </summary>
        /// <param name="y">row index</param>
        /// <param name="x">col index</param>
        public BoardVec2(int y, int x)
        {
            Y = y;
            X = x;
        }
        
        public static BoardVec2 operator +(BoardVec2 lhs, BoardVec2 rhs) => new(lhs.Y + rhs.Y, lhs.X + rhs.X);
        
        public static BoardVec2 operator *(BoardVec2 lhs, int rhs) => new(lhs.Y * rhs, lhs.X * rhs);
        
        public static BoardVec2 operator *(int lhs, BoardVec2 rhs) => new(rhs.Y * lhs, rhs.X * lhs);

        public bool Equals(BoardVec2 other) => Y == other.Y && X == other.X;

        public override bool Equals(object obj) => obj is BoardVec2 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Y, X);

        public override string ToString() => $"[{Y},{X}]";

        public static implicit operator string(BoardVec2 boardVec2) => boardVec2.ToString();

    }
}
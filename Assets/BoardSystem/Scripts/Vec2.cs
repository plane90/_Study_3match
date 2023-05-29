
namespace BoardSystem
{
    [System.Serializable]
    public struct Vec2
    {
        public int Y { get; set; }
        public int X { get; set; }

        public static Vec2 up = new(-1, 0); 
        public static Vec2 right = new(0, 1); 
        public static Vec2 down = new(1, 0); 
        public static Vec2 left = new(0, -1); 

        public Vec2(int y, int x)
        {
            Y = y;
            X = x;
        }
        
        public static Vec2 operator +(Vec2 lhs, Vec2 rhs) => new(lhs.Y + rhs.Y, lhs.X + rhs.X);
        
        public static Vec2 operator *(Vec2 lhs, int rhs) => new(lhs.Y * rhs, lhs.X * rhs);
        
        public static Vec2 operator *(int lhs, Vec2 rhs) => new(rhs.Y * lhs, rhs.X * lhs);

        public override string ToString() => $"({Y},{X})";

        public static implicit operator string(Vec2 vec2) => $"({vec2.Y},{vec2.X})";

    }
}
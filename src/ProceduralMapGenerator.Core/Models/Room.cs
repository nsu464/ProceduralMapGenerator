namespace ProceduralMapGenerator.Core.Models;

public class Room
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;

    public MapRect Bounds => new() { X = X, Y = Y, Width = Width, Height = Height };

    public Room(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Intersects(Room other) =>
        X - 1 < other.X + other.Width  &&
        X + Width  + 1 > other.X       &&
        Y - 1 < other.Y + other.Height &&
        Y + Height + 1 > other.Y;

    public bool Contains(int x, int y) =>
        x >= X && x < X + Width &&
        y >= Y && y < Y + Height;
}

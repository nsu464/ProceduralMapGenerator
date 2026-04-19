using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Generators;

internal class BspNode
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public BspNode? Left { get; set; }
    public BspNode? Right { get; set; }
    public Room? Room { get; set; }

    public bool IsLeaf => Left == null && Right == null;

    public BspNode(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

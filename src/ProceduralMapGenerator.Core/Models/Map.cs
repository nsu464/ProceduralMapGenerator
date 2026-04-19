namespace ProceduralMapGenerator.Core.Models;

public class Map
{
    public int Width { get; }
    public int Height { get; }
    public Cell[,] Cells { get; }
    public List<Room> Rooms { get; } = [];

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        Cells = new Cell[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Cells[x, y] = new Cell { X = x, Y = y, Type = TileType.Wall };
    }

    public Cell GetCell(int x, int y) => Cells[x, y];

    public void SetCell(int x, int y, TileType type) =>
        Cells[x, y] = new Cell { X = x, Y = y, Type = type };

    public bool InBounds(int x, int y) =>
        x >= 0 && x < Width && y >= 0 && y < Height;
}

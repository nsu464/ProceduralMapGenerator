namespace ProceduralMapGenerator.Core.Models;

public struct Cell
{
    public int X { get; init; }
    public int Y { get; init; }
    public TileType Type { get; set; }

    public readonly bool IsWalkable => Type is TileType.Floor or TileType.Door or TileType.Corridor;
}

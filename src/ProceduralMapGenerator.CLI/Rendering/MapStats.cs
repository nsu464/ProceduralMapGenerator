using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.CLI.Rendering;

public static class MapStats
{
    public static void Print(Map map, int seed)
    {
        int total     = map.Width * map.Height;
        int walkable  = 0;

        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var t = map.GetCell(x, y).Type;
                if (t is TileType.Floor or TileType.Corridor or TileType.Door)
                    walkable++;
            }

        double coverage = (double)walkable / total * 100.0;

        Console.WriteLine($"  Total rooms    : {map.Rooms.Count}");
        Console.WriteLine($"  Map size       : {map.Width}x{map.Height}");
        Console.WriteLine($"  Floor coverage : {coverage:F1}%");
        Console.WriteLine($"  Seed used      : {seed}");
    }
}

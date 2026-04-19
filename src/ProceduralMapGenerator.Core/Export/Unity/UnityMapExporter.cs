using System.Text.Json;
using ProceduralMapGenerator.Core.Generators;
using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Export.Unity;

public static class UnityMapExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string ToUnityJson(
        Map map, int seed, string generatorType,
        List<(int X, int Y)>? path = null, int nodesExplored = 0) =>
            JsonSerializer.Serialize(ToDto(map, seed, generatorType, path, nodesExplored), Options);

    public static void SaveToFile(
        Map map, int seed, string generatorType, string filePath,
        List<(int X, int Y)>? path = null, int nodesExplored = 0) =>
            File.WriteAllText(filePath, ToUnityJson(map, seed, generatorType, path, nodesExplored));

    // ── DTO mapping ───────────────────────────────────────────────────────────

    private static UnityMapExportDto ToDto(
        Map map, int seed, string generatorType,
        List<(int X, int Y)>? path, int nodesExplored)
    {
        var terrainMap = map as TerrainMap;

        // Flat row-major tile array (index = y * width + x)
        var tiles = new UnityTileDto[map.Width * map.Height];
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                var cell = map.GetCell(x, y);
                tiles[y * map.Width + x] = new UnityTileDto
                {
                    X          = x,
                    Y          = y,
                    TileType   = cell.Type.ToString(),
                    IsWalkable = cell.IsWalkable,
                    BiomeType  = terrainMap?.BiomeMap[x, y].ToString()
                };
            }
        }

        var rooms = map.Rooms.Select((r, i) => new UnityRoomDto
        {
            Id      = $"room_{i}",
            X       = r.X,
            Y       = r.Y,
            Width   = r.Width,
            Height  = r.Height,
            CenterX = r.CenterX,
            CenterY = r.CenterY
        }).ToArray();

        UnityPathDto? pathDto = null;
        if (path is { Count: > 0 })
        {
            pathDto = new UnityPathDto
            {
                Waypoints     = path.Select(p => new UnityVector2Int { X = p.X, Y = p.Y }).ToArray(),
                Length        = path.Count,
                NodesExplored = nodesExplored
            };
        }

        return new UnityMapExportDto
        {
            GeneratorType = generatorType,
            Width         = map.Width,
            Height        = map.Height,
            Seed          = seed,
            GeneratedAt   = DateTime.UtcNow.ToString("O"),
            Rooms         = rooms,
            Tiles         = tiles,
            LastPath      = pathDto
        };
    }
}

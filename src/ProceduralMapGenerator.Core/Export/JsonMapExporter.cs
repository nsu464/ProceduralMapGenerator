using System.Text.Json;
using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Export;

public static class JsonMapExporter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string ToJson(Map map, int seed) =>
        JsonSerializer.Serialize(ToDto(map, seed), Options);

    public static void SaveToFile(Map map, int seed, string filePath) =>
        File.WriteAllText(filePath, ToJson(map, seed));

    private static MapExportDto ToDto(Map map, int seed)
    {
        var cells = new CellExportDto[map.Height][];
        for (int y = 0; y < map.Height; y++)
        {
            cells[y] = new CellExportDto[map.Width];
            for (int x = 0; x < map.Width; x++)
            {
                var cell = map.GetCell(x, y);
                cells[y][x] = new CellExportDto
                {
                    X          = cell.X,
                    Y          = cell.Y,
                    Type       = cell.Type.ToString(),
                    IsWalkable = cell.IsWalkable
                };
            }
        }

        return new MapExportDto
        {
            Width       = map.Width,
            Height      = map.Height,
            Seed        = seed,
            GeneratedAt = DateTime.UtcNow.ToString("O"),
            Rooms       = map.Rooms.Select(r => new RoomExportDto
            {
                X       = r.X,
                Y       = r.Y,
                Width   = r.Width,
                Height  = r.Height,
                CenterX = r.CenterX,
                CenterY = r.CenterY
            }).ToList(),
            Cells = cells
        };
    }
}

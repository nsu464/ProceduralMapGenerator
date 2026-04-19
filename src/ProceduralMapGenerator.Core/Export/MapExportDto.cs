namespace ProceduralMapGenerator.Core.Export;

public class MapExportDto
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Seed { get; set; }
    public string GeneratedAt { get; set; } = string.Empty;
    public List<RoomExportDto> Rooms { get; set; } = [];
    public CellExportDto[][] Cells { get; set; } = [];
}

public class RoomExportDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int CenterX { get; set; }
    public int CenterY { get; set; }
}

public class CellExportDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsWalkable { get; set; }
}

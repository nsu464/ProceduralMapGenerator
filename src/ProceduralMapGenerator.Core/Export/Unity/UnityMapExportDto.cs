namespace ProceduralMapGenerator.Core.Export.Unity;

public class UnityMapExportDto
{
    public string GeneratorType { get; set; } = string.Empty;  // "BSP" | "Terrain"
    public int    Width         { get; set; }
    public int    Height        { get; set; }
    public int    Seed          { get; set; }
    public string GeneratedAt   { get; set; } = string.Empty;
    public UnityRoomDto[]  Rooms    { get; set; } = [];
    public UnityTileDto[]  Tiles    { get; set; } = [];        // flat row-major: index = y*width+x
    public UnityPathDto?   LastPath { get; set; }
}

public class UnityRoomDto
{
    public string Id      { get; set; } = string.Empty;        // "room_{index}"
    public int    X       { get; set; }
    public int    Y       { get; set; }
    public int    Width   { get; set; }
    public int    Height  { get; set; }
    public int    CenterX { get; set; }
    public int    CenterY { get; set; }
}

public class UnityTileDto
{
    public int    X          { get; set; }
    public int    Y          { get; set; }
    public string TileType   { get; set; } = string.Empty;     // TileType enum name
    public bool   IsWalkable { get; set; }
    public string? BiomeType { get; set; }                     // null for BSP maps
}

public class UnityPathDto
{
    public UnityVector2Int[] Waypoints     { get; set; } = [];
    public int               Length        { get; set; }
    public int               NodesExplored { get; set; }
}

public class UnityVector2Int
{
    public int X { get; set; }
    public int Y { get; set; }
}

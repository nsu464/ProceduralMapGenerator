namespace ProceduralMapGenerator.Core.Generators;

public class TerrainGeneratorOptions
{
    public int   Octaves     { get; set; } = 4;
    public float Persistence { get; set; } = 0.5f;
    public float Lacunarity  { get; set; } = 2.0f;
    public float Scale       { get; set; } = 0.05f;
    public bool  ApplyFalloff{ get; set; } = true;
}

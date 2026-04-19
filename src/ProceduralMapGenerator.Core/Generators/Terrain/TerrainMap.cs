using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Generators;

public class TerrainMap : Map
{
    public float[,]    HeightMap { get; }
    public BiomeType[,] BiomeMap { get; }

    public TerrainMap(int width, int height) : base(width, height)
    {
        HeightMap = new float[width, height];
        BiomeMap  = new BiomeType[width, height];
    }
}

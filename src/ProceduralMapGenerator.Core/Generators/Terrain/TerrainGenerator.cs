using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Generators;

public class TerrainGenerator : IMapGenerator
{
    private readonly TerrainGeneratorOptions _options;

    public TerrainGenerator(TerrainGeneratorOptions options)
    {
        _options = options;
    }

    public Map Generate(int width, int height, int seed)
    {
        var noise = new PerlinNoise(seed);
        var map   = new TerrainMap(width, height);

        // ── Height sampling ────────────────────────────────────────────────
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = noise.OctaveSample(
                    x * _options.Scale,
                    y * _options.Scale,
                    _options.Octaves,
                    _options.Persistence,
                    _options.Lacunarity);

                // Remap [-1, 1] → [0, 1]
                value = Math.Clamp((value + 1f) * 0.5f, 0f, 1f);

                if (_options.ApplyFalloff)
                    value *= RadialFalloff(x, y, width, height);

                map.HeightMap[x, y] = value;
            }
        }

        // ── Biome + tile assignment ────────────────────────────────────────
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var biome = HeightToBiome(map.HeightMap[x, y]);
                map.BiomeMap[x, y] = biome;
                map.SetCell(x, y, BiomeToTile(biome));
            }
        }

        return map;
    }

    // Smooth radial mask: 1.0 at centre, 0.0 at corners.
    private static float RadialFalloff(int x, int y, int width, int height)
    {
        float nx = (float)x / (width  - 1) * 2f - 1f;
        float ny = (float)y / (height - 1) * 2f - 1f;
        float d  = Math.Clamp(MathF.Sqrt(nx * nx + ny * ny) / MathF.Sqrt(2f), 0f, 1f);
        // Smoothstep inverse so the transition from land to water is gradual
        return 1f - d * d * (3f - 2f * d);
    }

    private static BiomeType HeightToBiome(float v) => v switch
    {
        < 0.20f => BiomeType.DeepWater,
        < 0.30f => BiomeType.ShallowWater,
        < 0.35f => BiomeType.Beach,
        < 0.55f => BiomeType.Grassland,
        < 0.70f => BiomeType.Forest,
        < 0.85f => BiomeType.Mountain,
        _       => BiomeType.Snow
    };

    private static TileType BiomeToTile(BiomeType biome) => biome switch
    {
        BiomeType.DeepWater or BiomeType.ShallowWater => TileType.Empty,
        BiomeType.Beach     or BiomeType.Grassland    => TileType.Floor,
        BiomeType.Forest    or BiomeType.Mountain     => TileType.Wall,
        _                                             => TileType.Door   // Snow
    };
}

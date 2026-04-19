using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Generators;

public interface IMapGenerator
{
    Map Generate(int width, int height, int seed);
}

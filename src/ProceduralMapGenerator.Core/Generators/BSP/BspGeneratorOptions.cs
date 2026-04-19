namespace ProceduralMapGenerator.Core.Generators;

public class BspGeneratorOptions
{
    public int MinRoomSize { get; set; } = 4;
    public int MaxRoomSize { get; set; } = 12;
    public int MinPartitionSize { get; set; } = 6;
    public float SplitRatio { get; set; } = 0.5f;
    public bool PlaceDoors { get; set; } = true;
}

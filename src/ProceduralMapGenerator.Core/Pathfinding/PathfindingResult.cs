namespace ProceduralMapGenerator.Core.Pathfinding;

public class PathfindingResult
{
    public List<(int X, int Y)> Path { get; init; } = [];
    public int    NodesExplored { get; init; }
    public int    PathLength    { get; init; }
    public double TimeMs        { get; init; }
    public bool   PathFound     => Path.Count > 0;
}

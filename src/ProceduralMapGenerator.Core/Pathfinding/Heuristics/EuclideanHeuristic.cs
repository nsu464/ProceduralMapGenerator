namespace ProceduralMapGenerator.Core.Pathfinding;

public class EuclideanHeuristic : IHeuristic
{
    public float Calculate(int x1, int y1, int x2, int y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

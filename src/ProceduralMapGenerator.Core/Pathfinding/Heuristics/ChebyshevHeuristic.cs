namespace ProceduralMapGenerator.Core.Pathfinding;

public class ChebyshevHeuristic : IHeuristic
{
    public float Calculate(int x1, int y1, int x2, int y2) =>
        Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
}

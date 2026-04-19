namespace ProceduralMapGenerator.Core.Pathfinding;

public class ManhattanHeuristic : IHeuristic
{
    public float Calculate(int x1, int y1, int x2, int y2) =>
        Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
}

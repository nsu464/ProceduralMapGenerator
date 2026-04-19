namespace ProceduralMapGenerator.Core.Pathfinding;

public interface IHeuristic
{
    float Calculate(int x1, int y1, int x2, int y2);
}

namespace ProceduralMapGenerator.Core.Pathfinding;

internal class PathNode
{
    public int X { get; }
    public int Y { get; }

    public float GCost { get; set; } = float.MaxValue;
    public float HCost { get; set; }
    public float FCost  => GCost + HCost;

    public PathNode? Parent { get; set; }
    public bool IsWalkable { get; }

    public PathNode(int x, int y, bool isWalkable)
    {
        X          = x;
        Y          = y;
        IsWalkable = isWalkable;
    }
}

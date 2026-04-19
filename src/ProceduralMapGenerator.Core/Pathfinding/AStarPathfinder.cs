using System.Diagnostics;
using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Pathfinding;

public class AStarPathfinder
{
    private static readonly (int dx, int dy)[] Directions = [(0, 1), (0, -1), (1, 0), (-1, 0)];

    private readonly IHeuristic _heuristic;

    public AStarPathfinder(IHeuristic? heuristic = null)
    {
        _heuristic = heuristic ?? new ManhattanHeuristic();
    }

    public List<(int X, int Y)> FindPath(Map map, int startX, int startY, int endX, int endY) =>
        FindPathWithStats(map, startX, startY, endX, endY).Path;

    public PathfindingResult FindPathWithStats(Map map, int startX, int startY, int endX, int endY)
    {
        var sw = Stopwatch.StartNew();
        var (path, explored) = RunAStar(map, startX, startY, endX, endY);
        sw.Stop();

        return new PathfindingResult
        {
            Path          = path,
            NodesExplored = explored,
            PathLength    = path.Count,
            TimeMs        = sw.Elapsed.TotalMilliseconds
        };
    }

    // ── Core algorithm ────────────────────────────────────────────────────────

    private (List<(int X, int Y)> Path, int NodesExplored) RunAStar(
        Map map, int startX, int startY, int endX, int endY)
    {
        if (!map.InBounds(startX, startY) || !map.InBounds(endX, endY))
            return ([], 0);

        // Build a fresh node grid for this search
        var nodes = new PathNode[map.Width, map.Height];
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                nodes[x, y] = new PathNode(x, y, map.GetCell(x, y).IsWalkable);

        var start = nodes[startX, startY];
        var end   = nodes[endX,   endY];

        if (!start.IsWalkable || !end.IsWalkable)
            return ([], 0);

        start.GCost = 0f;
        start.HCost = _heuristic.Calculate(startX, startY, endX, endY);

        // Min-heap: lowest FCost dequeued first
        var openQueue = new PriorityQueue<PathNode, float>();
        var closedSet = new HashSet<(int, int)>();
        openQueue.Enqueue(start, start.FCost);

        while (openQueue.Count > 0)
        {
            var current = openQueue.Dequeue();

            // Lazy deletion: skip nodes already committed to closed set
            if (!closedSet.Add((current.X, current.Y)))
                continue;

            if (current.X == endX && current.Y == endY)
                return (ReconstructPath(current), closedSet.Count);

            foreach (var (dx, dy) in Directions)
            {
                int nx = current.X + dx;
                int ny = current.Y + dy;

                if (!map.InBounds(nx, ny)) continue;

                var neighbor = nodes[nx, ny];
                if (!neighbor.IsWalkable || closedSet.Contains((nx, ny))) continue;

                float tentativeG = current.GCost + 1f;
                if (tentativeG < neighbor.GCost)
                {
                    neighbor.GCost  = tentativeG;
                    neighbor.HCost  = _heuristic.Calculate(nx, ny, endX, endY);
                    neighbor.Parent = current;
                    openQueue.Enqueue(neighbor, neighbor.FCost);
                }
            }
        }

        return ([], closedSet.Count); // No path found
    }

    private static List<(int X, int Y)> ReconstructPath(PathNode endNode)
    {
        var path    = new List<(int X, int Y)>();
        var current = endNode;
        while (current is not null)
        {
            path.Add((current.X, current.Y));
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }
}

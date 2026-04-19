using FluentAssertions;
using ProceduralMapGenerator.Core.Generators;
using ProceduralMapGenerator.Core.Models;
using ProceduralMapGenerator.Core.Pathfinding;

namespace ProceduralMapGenerator.Core.Tests.Pathfinding;

public class AStarPathfinderTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    // All cells set to Floor so every position is walkable.
    private static Map CreateOpenMap(int width, int height)
    {
        var map = new Map(width, height);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map.SetCell(x, y, TileType.Floor);
        return map;
    }

    // Open map with a solid wall column at wallX covering y = 0..wallY (inclusive).
    private static Map CreateMapWithWall(int width, int height, int wallX, int wallMaxY)
    {
        var map = CreateOpenMap(width, height);
        for (int y = 0; y <= wallMaxY; y++)
            map.SetCell(wallX, y, TileType.Wall);
        return map;
    }

    // ── 1 ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void FindPath_SimpleOpenMap_ReturnsShortestPath()
    {
        var map    = CreateOpenMap(10, 10);
        var result = new AStarPathfinder().FindPath(map, 0, 0, 5, 0);

        // Straight horizontal path: 5 steps → 6 nodes (start inclusive)
        result.Should().HaveCount(6);
        result[0].Should().Be((0, 0));
        result[^1].Should().Be((5, 0));
    }

    // ── 2 ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void FindPath_NoPathExists_ReturnsEmptyList()
    {
        // Wall column at x=2 spans full height → completely blocks left from right.
        var map = CreateOpenMap(5, 3);
        for (int y = 0; y < 3; y++)
            map.SetCell(2, y, TileType.Wall);

        var result = new AStarPathfinder().FindPath(map, 0, 1, 4, 1);

        result.Should().BeEmpty();
    }

    // ── 3 ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void FindPath_StartEqualsEnd_ReturnsSingleNode()
    {
        var map    = CreateOpenMap(5, 5);
        var result = new AStarPathfinder().FindPath(map, 2, 2, 2, 2);

        result.Should().HaveCount(1);
        result[0].Should().Be((2, 2));
    }

    // ── 4 ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void FindPath_WithWalls_NavigatesAround()
    {
        // Wall at x=2, y=0..3 leaves only (2,4) as a gap at the bottom.
        // Path from (0,0) to (4,0) must detour all the way down and back up.
        var map = CreateMapWithWall(5, 5, wallX: 2, wallMaxY: 3);

        var result = new AStarPathfinder().FindPath(map, 0, 0, 4, 0);

        result.Should().NotBeEmpty("a path via the bottom gap must exist");
        result[0].Should().Be((0, 0));
        result[^1].Should().Be((4, 0));

        // Every step in the path must be walkable
        result.Should().AllSatisfy(p =>
            map.GetCell(p.X, p.Y).IsWalkable.Should().BeTrue($"cell ({p.X},{p.Y}) must be walkable"));

        // Path must cross the wall column at y=4 (the only gap)
        result.Should().Contain((2, 4), "the only crossing point is the gap at (2,4)");
    }

    // ── 5 ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void FindPath_DeterministicResult_SameSeedSamePath()
    {
        var gen  = new BspDungeonGenerator(new BspGeneratorOptions());
        var map1 = gen.Generate(60, 30, seed: 42);
        var map2 = gen.Generate(60, 30, seed: 42);

        // Both maps must be structurally identical (covered by dungeon tests),
        // so paths between the same endpoints must also be identical.
        var pathfinder = new AStarPathfinder();
        var start      = map1.Rooms[0];
        var end        = map1.Rooms[^1];

        var path1 = pathfinder.FindPath(map1, start.CenterX, start.CenterY, end.CenterX, end.CenterY);
        var path2 = pathfinder.FindPath(map2, start.CenterX, start.CenterY, end.CenterX, end.CenterY);

        path1.Should().Equal(path2, "identical maps and endpoints must yield identical paths");
    }

    // ── 6 ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void FindPathWithStats_TracksNodesExplored()
    {
        var map    = CreateOpenMap(20, 20);
        var result = new AStarPathfinder().FindPathWithStats(map, 0, 0, 19, 19);

        result.PathFound.Should().BeTrue();
        result.PathLength.Should().Be(result.Path.Count);
        result.NodesExplored.Should().BeGreaterThan(0,
            "at least the start node must have been committed to the closed set");
        result.TimeMs.Should().BeGreaterThanOrEqualTo(0);

        // On an open grid the Manhattan-optimal path is 38 steps (19+19).
        result.PathLength.Should().Be(39, "Manhattan distance from (0,0) to (19,19) is 38 → 39 nodes");
    }
}

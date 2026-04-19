using FluentAssertions;
using ProceduralMapGenerator.Core.Generators;
using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.Core.Tests.Generators;

public class BspDungeonGeneratorTests
{
    private static BspDungeonGenerator Default(bool placeDoors = true) =>
        new(new BspGeneratorOptions { PlaceDoors = placeDoors });

    private static IEnumerable<Cell> AllCells(Map map) =>
        Enumerable.Range(0, map.Width)
            .SelectMany(x => Enumerable.Range(0, map.Height)
                .Select(y => map.GetCell(x, y)));

    // ── 1 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_WithFixedSeed_ReturnsDeterministicMap()
    {
        var gen  = Default();
        var map1 = gen.Generate(80, 50, seed: 42);
        var map2 = gen.Generate(80, 50, seed: 42);

        for (int x = 0; x < map1.Width; x++)
            for (int y = 0; y < map1.Height; y++)
                map1.GetCell(x, y).Type.Should().Be(
                    map2.GetCell(x, y).Type,
                    $"cell ({x},{y}) must be identical for the same seed");
    }

    // ── 2 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_MapDimensions_MatchRequested()
    {
        var map = Default().Generate(60, 40, seed: 1);

        map.Width.Should().Be(60);
        map.Height.Should().Be(40);
    }

    // ── 3 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_AllCells_AreInitialized()
    {
        var map        = Default().Generate(60, 40, seed: 1);
        var validTypes = Enum.GetValues<TileType>().ToHashSet();

        AllCells(map)
            .Select(c => c.Type)
            .Should().AllSatisfy(t => validTypes.Should().Contain(t));
    }

    // ── 4 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_HasAtLeastOneRoom()
    {
        var map = Default().Generate(60, 40, seed: 1);

        map.Rooms.Should().NotBeEmpty();
    }

    // ── 5 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_RoomsDoNotOverlap()
    {
        var map = Default().Generate(80, 60, seed: 7);

        for (int i = 0; i < map.Rooms.Count; i++)
            for (int j = i + 1; j < map.Rooms.Count; j++)
                map.Rooms[i].Intersects(map.Rooms[j]).Should().BeFalse(
                    $"room[{i}] and room[{j}] must not overlap (BSP guarantees per-partition placement)");
    }

    // ── 6 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_FloorCells_AreInsideMapBounds()
    {
        var map = Default().Generate(60, 40, seed: 3);

        var floorCells = AllCells(map).Where(c => c.Type == TileType.Floor);

        floorCells.Should().AllSatisfy(c =>
        {
            c.X.Should().BeInRange(0, map.Width  - 1);
            c.Y.Should().BeInRange(0, map.Height - 1);
        });
    }

    // ── 7 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_CorridorCells_ConnectRooms()
    {
        // Use a large map to ensure multiple rooms are generated.
        var map = Default(placeDoors: false).Generate(80, 60, seed: 5);

        map.Rooms.Count.Should().BeGreaterThan(1,
            "this test requires at least two rooms to assert corridor existence");

        AllCells(map)
            .Should().Contain(c => c.Type == TileType.Corridor,
                "corridors must be carved to connect the rooms");
    }

    // ── 8 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_DifferentSeeds_ProduceDifferentMaps()
    {
        var gen  = Default();
        var map1 = gen.Generate(80, 50, seed: 1);
        var map2 = gen.Generate(80, 50, seed: 2);

        bool identical = Enumerable.Range(0, map1.Width).All(x =>
            Enumerable.Range(0, map1.Height).All(y =>
                map1.GetCell(x, y).Type == map2.GetCell(x, y).Type));

        identical.Should().BeFalse("two different seeds must produce different layouts");
    }

    // ── 9 ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_WithDoorOption_PlacesDoors()
    {
        var map = new BspDungeonGenerator(new BspGeneratorOptions { PlaceDoors = true })
            .Generate(80, 60, seed: 1);

        AllCells(map)
            .Should().Contain(c => c.Type == TileType.Door,
                "at least one door must appear when PlaceDoors=true and rooms are connected");
    }

    // ── 10 ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_SmallMap_DoesNotThrow()
    {
        var act = () => Default().Generate(20, 20, seed: 99);

        act.Should().NotThrow();
    }
}

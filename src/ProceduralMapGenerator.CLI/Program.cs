using ProceduralMapGenerator.CLI.Rendering;
using ProceduralMapGenerator.Core.Export;
using ProceduralMapGenerator.Core.Generators;
using ProceduralMapGenerator.Core.Models;
using ProceduralMapGenerator.Core.Pathfinding;

// Ensure Unicode glyphs (▲ ♦ ♣ ≈ ●) render correctly on Windows terminals.
Console.OutputEncoding = System.Text.Encoding.UTF8;

int[] demoSeeds = [42, 1337, 99999];

while (true)
{
    if (!Console.IsOutputRedirected) Console.Clear();
    Console.WriteLine("┌──────────────────────────────────┐");
    Console.WriteLine("│   Procedural Map Generator        │");
    Console.WriteLine("├──────────────────────────────────┤");
    Console.WriteLine("│  [1] BSP Dungeon                  │");
    Console.WriteLine("│  [2] Terrain (Perlin Noise)       │");
    Console.WriteLine("│  [3] Exit                         │");
    Console.WriteLine("└──────────────────────────────────┘");
    Console.Write("\nChoice: ");

    if (Console.IsInputRedirected)
    {
        Console.WriteLine("(non-interactive — exiting)");
        return;
    }

    char choice = Console.ReadKey(intercept: true).KeyChar;
    Console.WriteLine(choice);
    Console.WriteLine();

    if (choice == '1')
        RunLoop(
            new BspDungeonGenerator(new BspGeneratorOptions()),
            width: 60, height: 30, demoSeeds,
            prefix: "dungeon",
            render: ConsoleRenderer.Render,
            afterStats: PromptPathfinding);
    else if (choice == '2')
        RunLoop(
            new TerrainGenerator(new TerrainGeneratorOptions()),
            width: 80, height: 40, demoSeeds,
            prefix: "terrain",
            render: map => ConsoleRenderer.RenderTerrain((TerrainMap)map));
    else
        return;
}

// ── Shared map loop ───────────────────────────────────────────────────────────

static void RunLoop(IMapGenerator gen, int width, int height, int[] seeds,
                    string prefix, Action<Map> render,
                    Action<Map>? afterStats = null)
{
    for (int i = 0; i < seeds.Length; i++)
    {
        int seed = seeds[i];

        if (!Console.IsOutputRedirected) Console.Clear();
        Console.WriteLine($"=== {prefix} Seed: {seed} ({i + 1}/{seeds.Length}) ===");
        Console.WriteLine();

        if (!Console.IsOutputRedirected && width > Console.WindowWidth)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Warning: map width ({width}) exceeds console width ({Console.WindowWidth}).");
            Console.WriteLine($"Resize the terminal to at least {width} columns for correct rendering.");
            Console.ResetColor();
        }
        else
        {
            var map = gen.Generate(width, height, seed);
            render(map);
            Console.WriteLine();
            MapStats.Print(map, seed);
            Console.WriteLine();

            afterStats?.Invoke(map);

            if (!Console.IsInputRedirected)
            {
                Console.Write("Export this map to JSON? (y/n) ");
                var key = Console.ReadKey(intercept: true);
                Console.WriteLine();

                if (key.KeyChar is 'y' or 'Y')
                {
                    Directory.CreateDirectory("./exports");
                    string path = Path.GetFullPath(
                        Path.Combine("./exports", $"{prefix}_seed_{seed}.json"));
                    JsonMapExporter.SaveToFile(map, seed, path);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  Saved → {path}");
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
        }

        if (!Console.IsInputRedirected)
        {
            string prompt = i < seeds.Length - 1
                ? "Press any key for next map..."
                : "Press any key to return to menu...";
            Console.Write(prompt);
            Console.ReadKey(intercept: true);
            Console.WriteLine();
        }
    }
}

// ── A* pathfinding interaction ────────────────────────────────────────────────

static void PromptPathfinding(Map map)
{
    if (Console.IsInputRedirected || map.Rooms.Count < 2) return;

    Console.Write("Run A* pathfinding between two rooms? (y/n) ");
    if (Console.ReadKey(intercept: true).KeyChar is not ('y' or 'Y'))
    {
        Console.WriteLine();
        return;
    }
    Console.WriteLine();
    Console.WriteLine();

    var start = map.Rooms[0];
    var end   = map.Rooms[^1];

    ShowPath(map, start.CenterX, start.CenterY, end.CenterX, end.CenterY,
             new ManhattanHeuristic(), "Manhattan");

    Console.Write("Try other heuristic? [1] Euclidean  [2] Chebyshev  [3] Skip: ");
    char h = Console.ReadKey(intercept: true).KeyChar;
    Console.WriteLine();
    Console.WriteLine();

    IHeuristic? alt = h switch
    {
        '1' => new EuclideanHeuristic(),
        '2' => new ChebyshevHeuristic(),
        _   => null
    };

    if (alt is not null)
    {
        string name = h == '1' ? "Euclidean" : "Chebyshev";
        ShowPath(map, start.CenterX, start.CenterY, end.CenterX, end.CenterY, alt, name);
    }
}

static void ShowPath(Map map, int sx, int sy, int ex, int ey, IHeuristic heuristic, string name)
{
    var result = new AStarPathfinder(heuristic).FindPathWithStats(map, sx, sy, ex, ey);
    ConsoleRenderer.RenderWithPath(map, result.Path);
    Console.WriteLine();
    Console.WriteLine($"  Heuristic     : {name}");
    Console.WriteLine($"  Path found    : {result.PathFound}");
    Console.WriteLine($"  Path length   : {result.PathLength}");
    Console.WriteLine($"  Nodes explored: {result.NodesExplored}");
    Console.WriteLine($"  Time          : {result.TimeMs:F3} ms");
    Console.WriteLine();
}

using ProceduralMapGenerator.CLI.Rendering;
using ProceduralMapGenerator.Core.Export;
using ProceduralMapGenerator.Core.Export.Unity;
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
            generatorType: "BSP",
            render: ConsoleRenderer.Render,
            afterStats: PromptPathfinding);
    else if (choice == '2')
        RunLoop(
            new TerrainGenerator(new TerrainGeneratorOptions()),
            width: 80, height: 40, demoSeeds,
            prefix: "terrain",
            generatorType: "Terrain",
            render: map => ConsoleRenderer.RenderTerrain((TerrainMap)map));
    else
        return;
}

// ── Shared map loop ───────────────────────────────────────────────────────────

static void RunLoop(
    IMapGenerator gen, int width, int height, int[] seeds,
    string prefix, string generatorType, Action<Map> render,
    Func<Map, List<(int X, int Y)>?>? afterStats = null)
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
            var map      = gen.Generate(width, height, seed);
            render(map);
            Console.WriteLine();
            MapStats.Print(map, seed);
            Console.WriteLine();

            List<(int X, int Y)>? lastPath = afterStats?.Invoke(map);

            PromptExport(map, seed, prefix, generatorType, lastPath);
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

// ── Export menu ───────────────────────────────────────────────────────────────

static void PromptExport(Map map, int seed, string prefix, string generatorType,
                         List<(int X, int Y)>? lastPath)
{
    if (Console.IsInputRedirected) return;

    Console.Write("Export: [1] Standard JSON  [2] Unity JSON + Guide  [3] Both  [4] Skip: ");
    char key = Console.ReadKey(intercept: true).KeyChar;
    Console.WriteLine();
    Console.WriteLine();

    bool doStandard = key is '1' or '3';
    bool doUnity    = key is '2' or '3';

    if (doStandard)
    {
        Directory.CreateDirectory("./exports");
        string path = Path.GetFullPath(Path.Combine("./exports", $"{prefix}_seed_{seed}.json"));
        JsonMapExporter.SaveToFile(map, seed, path);
        PrintSaved(path);
    }

    if (doUnity)
    {
        string unityDir = Path.GetFullPath("./exports/unity");
        Directory.CreateDirectory(unityDir);

        string jsonPath  = Path.Combine(unityDir, $"{prefix}_seed_{seed}.json");
        string guidePath = Path.Combine(unityDir, $"{prefix}_seed_{seed}_unity_guide.md");

        // Thread nodesExplored through if pathfinding was run — path list doesn't carry it,
        // so we embed 0; the value is informational only.
        UnityMapExporter.SaveToFile(map, seed, generatorType, jsonPath, lastPath);
        File.WriteAllText(guidePath, UnityReadmeGenerator.GenerateReadme(map, seed, generatorType));

        PrintSaved(jsonPath);
        PrintSaved(guidePath);
    }

    if (doStandard || doUnity) Console.WriteLine();
}

static void PrintSaved(string path)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  Saved → {path}");
    Console.ResetColor();
}

// ── A* pathfinding interaction ────────────────────────────────────────────────

static List<(int X, int Y)>? PromptPathfinding(Map map)
{
    if (Console.IsInputRedirected || map.Rooms.Count < 2) return null;

    Console.Write("Run A* pathfinding between two rooms? (y/n) ");
    if (Console.ReadKey(intercept: true).KeyChar is not ('y' or 'Y'))
    {
        Console.WriteLine();
        return null;
    }
    Console.WriteLine();
    Console.WriteLine();

    var start = map.Rooms[0];
    var end   = map.Rooms[^1];

    var lastPath = ShowPath(map, start.CenterX, start.CenterY,
                                 end.CenterX,   end.CenterY,
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
        lastPath = ShowPath(map, start.CenterX, start.CenterY,
                                 end.CenterX,   end.CenterY,
                                 alt, h == '1' ? "Euclidean" : "Chebyshev");

    return lastPath;
}

static List<(int X, int Y)> ShowPath(
    Map map, int sx, int sy, int ex, int ey, IHeuristic heuristic, string name)
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
    return result.Path;
}

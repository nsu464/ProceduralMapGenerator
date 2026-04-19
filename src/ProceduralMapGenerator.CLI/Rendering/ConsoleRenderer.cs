using ProceduralMapGenerator.Core.Generators;
using ProceduralMapGenerator.Core.Models;

namespace ProceduralMapGenerator.CLI.Rendering;

public static class ConsoleRenderer
{
    private static readonly (char Glyph, ConsoleColor Color)[] TileStyle =
        new (char, ConsoleColor)[Enum.GetValues<TileType>().Length];

    static ConsoleRenderer()
    {
        TileStyle[(int)TileType.Empty]    = (' ', ConsoleColor.Black);
        TileStyle[(int)TileType.Floor]    = ('·', ConsoleColor.White);
        TileStyle[(int)TileType.Wall]     = ('█', ConsoleColor.DarkGray);
        TileStyle[(int)TileType.Door]     = ('+', ConsoleColor.Cyan);
        TileStyle[(int)TileType.Corridor] = (' ', ConsoleColor.Yellow);
    }

    public static void Render(Map map)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                var (glyph, color) = TileStyle[(int)map.GetCell(x, y).Type];
                Console.ForegroundColor = color;
                Console.Write(glyph);
            }
            Console.WriteLine();
        }

        Console.ResetColor();
        PrintLegend();
    }

    private static void PrintLegend()
    {
        Console.WriteLine();
        Console.Write("Legend: ");
        WriteLegendEntry('█', ConsoleColor.DarkGray, "Wall");
        Console.Write("  ");
        WriteLegendEntry('·', ConsoleColor.White, "Floor");
        Console.Write("  ");
        WriteLegendEntry(' ', ConsoleColor.Yellow, "Corridor", highlight: true);
        Console.Write("  ");
        WriteLegendEntry('+', ConsoleColor.Cyan, "Door");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void WriteLegendEntry(char glyph, ConsoleColor color, string label, bool highlight = false)
    {
        if (highlight)
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.Black;
        }
        else
        {
            Console.ForegroundColor = color;
        }

        Console.Write(glyph);
        Console.ResetColor();
        Console.Write($" {label}");
    }

    // ── Terrain rendering ────────────────────────────────────────────────────

    private static readonly (char Glyph, ConsoleColor Color)[] BiomeStyle =
    [
        ('≈', ConsoleColor.DarkBlue),   // DeepWater
        ('~', ConsoleColor.Blue),        // ShallowWater
        ('.', ConsoleColor.Yellow),      // Beach
        ('♦', ConsoleColor.Green),       // Grassland
        ('♣', ConsoleColor.DarkGreen),   // Forest
        ('▲', ConsoleColor.DarkGray),    // Mountain
        ('*', ConsoleColor.White),       // Snow
    ];

    public static void RenderTerrain(TerrainMap map)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                var (glyph, color) = BiomeStyle[(int)map.BiomeMap[x, y]];
                Console.ForegroundColor = color;
                Console.Write(glyph);
            }
            Console.WriteLine();
        }

        Console.ResetColor();
        PrintTerrainLegend();
    }

    private static void PrintTerrainLegend()
    {
        Console.WriteLine();
        Console.Write("Legend: ");
        (char, ConsoleColor, string)[] entries =
        [
            ('≈', ConsoleColor.DarkBlue,  "DeepWater"),
            ('~', ConsoleColor.Blue,       "ShallowWater"),
            ('.', ConsoleColor.Yellow,     "Beach"),
            ('♦', ConsoleColor.Green,      "Grassland"),
            ('♣', ConsoleColor.DarkGreen,  "Forest"),
            ('▲', ConsoleColor.DarkGray,   "Mountain"),
            ('*', ConsoleColor.White,      "Snow"),
        ];
        foreach (var (glyph, color, label) in entries)
        {
            WriteLegendEntry(glyph, color, label);
            Console.Write("  ");
        }
        Console.ResetColor();
        Console.WriteLine();
    }
}

# ProceduralMapGenerator

A deterministic procedural map generation library for games, written in pure C# (.NET 8).
Implements BSP dungeon generation, Perlin Noise terrain, and A* pathfinding — no engine dependencies, Unity-ready export.

---

## Features

- **BSP Dungeon Generator** — binary space partitioning with seeded randomness, room placement, L-corridor carving and automatic door detection
- **Perlin Noise Terrain** — multi-octave noise with radial falloff (island mask) and 7 biome types
- **A\* Pathfinding** — pluggable heuristics (Manhattan, Euclidean, Chebyshev), per-search stats, fully deterministic results
- **Unity Export** — camelCase JSON schema with ready-to-use `[Serializable]` C# structs and a `MonoBehaviour` integration guide auto-generated per map
- **CLI Visualizer** — full ANSI-color console renderer with per-tile characters, legend, and map stats
- **Deterministic** — the same seed always produces identical output, cross-platform

---

## Architecture

The solution follows a clean layered architecture: **Core** contains all algorithms and domain models with zero external dependencies — pure C# structs, classes, and interfaces. **CLI** is a thin consumer that wires together generation, rendering, pathfinding, and export. The **test project** exercises Core directly with xUnit and FluentAssertions, with no mocking required because all generators are pure functions seeded by an integer.

```
ProceduralMapGenerator/
├── ProceduralMapGenerator.sln
├── src/
│   ├── ProceduralMapGenerator.Core/          # Zero-dependency library
│   │   ├── Models/                           # Cell, Map, Room, TileType, MapRect
│   │   ├── Generators/
│   │   │   ├── Interfaces/IMapGenerator.cs
│   │   │   ├── BSP/                          # BspDungeonGenerator, BspNode, BspGeneratorOptions
│   │   │   ├── Noise/PerlinNoise.cs
│   │   │   └── Terrain/                      # TerrainGenerator, TerrainMap, BiomeType, options
│   │   ├── Pathfinding/
│   │   │   ├── AStarPathfinder.cs
│   │   │   ├── IHeuristic.cs, PathNode.cs, PathfindingResult.cs
│   │   │   └── Heuristics/                   # Manhattan, Euclidean, Chebyshev
│   │   └── Export/
│   │       ├── JsonMapExporter.cs            # Standard indented JSON
│   │       └── Unity/                        # UnityMapExporter, UnityReadmeGenerator, DTOs
│   └── ProceduralMapGenerator.CLI/           # Console entry point
│       └── Rendering/                        # ConsoleRenderer, MapStats
└── tests/
    └── ProceduralMapGenerator.Core.Tests/
        ├── Generators/BspDungeonGeneratorTests.cs
        └── Pathfinding/AStarPathfinderTests.cs
```

---

## Quick Start

```bash
git clone https://github.com/nsu464/ProceduralMapGenerator.git
cd ProceduralMapGenerator
dotnet run --project src/ProceduralMapGenerator.CLI
```

Select **[1] BSP Dungeon** or **[2] Terrain** from the menu. After each map renders you can run A\* pathfinding between rooms, then export to standard JSON or Unity JSON + integration guide.

---

## Usage — Library API

**Generate a BSP dungeon**
```csharp
var generator = new BspDungeonGenerator(new BspGeneratorOptions());
Map map = generator.Generate(width: 80, height: 50, seed: 42);
// map.Rooms, map.Cells, map.Width, map.Height are all populated
```

**Generate Perlin Noise terrain**
```csharp
var generator = new TerrainGenerator(new TerrainGeneratorOptions { ApplyFalloff = true });
var terrain   = (TerrainMap)generator.Generate(width: 120, height: 80, seed: 1337);
// terrain.BiomeMap[x, y] → BiomeType enum; terrain.HeightMap[x, y] → float 0..1
```

**Run A\* pathfinding**
```csharp
var pathfinder = new AStarPathfinder(new ManhattanHeuristic());
PathfindingResult result = pathfinder.FindPathWithStats(map, startX: 3, startY: 5, endX: 55, endY: 24);
// result.Path, result.PathLength, result.NodesExplored, result.TimeMs
```

**Export to Unity JSON**
```csharp
UnityMapExporter.SaveToFile(map, seed: 42, generatorType: "BSP", filePath: "dungeon.json");
File.WriteAllText("dungeon_unity_guide.md", UnityReadmeGenerator.GenerateReadme(map, seed: 42, "BSP"));
```

---

## Algorithms

### BSP Dungeon

The map is recursively partitioned into a binary tree of axis-aligned rectangles. At each node the algorithm chooses the longer axis to split (with ±20% random variance around the configured `SplitRatio`) and stops when a partition falls below `MinPartitionSize`. One room is placed inside each leaf partition with 1-cell padding and randomised position. Rooms are then connected bottom-up: for each internal node an L-shaped corridor (horizontal-first or vertical-first, chosen randomly) links a representative room from each child subtree. A final pass promotes any `Floor` cell adjacent to a `Corridor` cell to `Door`.

### Perlin Noise Terrain

Classic Ken Perlin 2D noise with a Fisher-Yates-shuffled permutation table seeded from the constructor integer. `OctaveSample` accumulates `octaves` layers of noise at increasing frequencies (`lacunarity`) and decreasing amplitudes (`persistence`), then normalises by total amplitude. The `[0, 1]` result is optionally multiplied by a radial falloff mask (smooth-step inverse, Euclidean distance from centre normalised by the diagonal) to produce an island silhouette. Seven biome thresholds map height to `DeepWater → ShallowWater → Beach → Grassland → Forest → Mountain → Snow`.

### A\* Pathfinding

A standard A\* over a `PathNode[width, height]` grid built fresh for each search. The open set is a .NET `PriorityQueue<PathNode, float>` (min-heap on FCost = GCost + HCost); stale entries are discarded via lazy deletion against a `HashSet` closed set. Movement is 4-directional with unit cost. `ManhattanHeuristic` is admissible for 4-directional grids and produces optimal paths; `EuclideanHeuristic` tends to explore fewer nodes in open spaces at the cost of optimality; `ChebyshevHeuristic` is included for 8-directional comparison. `FindPathWithStats` wraps the search in a `Stopwatch` and returns explored-node count alongside the path.

---

## Export Format

Unity JSON uses `JsonNamingPolicy.CamelCase`. Tiles are stored as a flat row-major array (`index = y * width + x`) for direct use with Unity arrays. The optional `lastPath` field embeds the A\* waypoints when pathfinding was run before export.

```json
{
  "generatorType": "BSP",
  "width": 60,
  "height": 30,
  "seed": 42,
  "generatedAt": "2026-04-19T17:53:04.779Z",
  "rooms": [
    { "id": "room_0", "x": 1, "y": 3, "width": 4, "height": 5, "centerX": 3, "centerY": 5 }
  ],
  "tiles": [
    { "x": 0, "y": 0, "tileType": "Wall",  "isWalkable": false, "biomeType": null },
    { "x": 1, "y": 0, "tileType": "Wall",  "isWalkable": false, "biomeType": null },
    { "x": 2, "y": 0, "tileType": "Floor", "isWalkable": true,  "biomeType": null }
  ],
  "lastPath": null
}
```

Each export also saves a `*_unity_guide.md` alongside the JSON containing copy-paste `[Serializable]` C# structs, a complete `LoadMap` MonoBehaviour with Tilemap integration, and tips for batch painting, colliders, and biome layers.

---

## Test Coverage

| Test class | Tests | What it covers |
|---|---|---|
| `BspDungeonGeneratorTests` | 10 | Determinism, dimensions, room count, overlap, floor bounds, corridors, doors, small-map safety |
| `AStarPathfinderTests` | 6 | Shortest path, no-path detection, start-equals-end, wall navigation, determinism, stats tracking |

Run all tests:

```bash
dotnet test
```

---

## Roadmap

- [ ] Wave Function Collapse generator
- [ ] Dijkstra flood-fill for reachability analysis
- [ ] Unity UPM package distribution
- [ ] Unreal Engine JSON importer (Blueprint-compatible)
- [ ] Web visualizer (Blazor WebAssembly)

---

## Tech Stack

| | |
|---|---|
| **Language** | C# 12 |
| **Framework** | .NET 8 |
| **Test framework** | xUnit 2.9 + FluentAssertions 8.9 |
| **Serialization** | System.Text.Json (built-in) |
| **Export targets** | Standard JSON, Unity-compatible JSON + Markdown guide |
| **Dependencies** | Zero runtime NuGet packages in Core |

---

## Author

**Noel Solana Ubes** — Software Engineer

- GitHub: [github.com/nsu464](https://github.com/nsu464)
- LinkedIn: [linkedin.com/in/noel-solana-ubes-102b77248](https://linkedin.com/in/noel-solana-ubes-102b77248/)

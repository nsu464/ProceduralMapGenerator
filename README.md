# ProceduralMapGenerator

A procedural map generation library written in C# (.NET 8).

## Description

Placeholder — procedural map generation algorithms will live in `ProceduralMapGenerator.Core`.

## Structure

```
ProceduralMapGenerator/
├── src/
│   ├── ProceduralMapGenerator.Core/        # Pure algorithms, no external dependencies
│   └── ProceduralMapGenerator.CLI/         # Console entry point for testing
└── tests/
    └── ProceduralMapGenerator.Core.Tests/  # xUnit + FluentAssertions
```

## Getting Started

```bash
dotnet build
dotnet run --project src/ProceduralMapGenerator.CLI
dotnet test
```

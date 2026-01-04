# Agent Instructions

This document provides context for AI coding assistants working on this project.

## Project Overview

This is a **Godot 4 C#** procedural maze generation project. It generates 3D mazes using various algorithms and provides visualization and solving capabilities.

## Build Commands

```bash
# Build the main project
dotnet build

# Build and run tests
cd tests && dotnet test

# Run tests with coverage
cd tests && dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./coverage/

# Run specific test category
cd tests && dotnet test --filter "FullyQualifiedName~AgentTests"

# Run experiments (benchmarking/analysis)
cd experiments && dotnet run

# Run benchmarks (performance testing)
cd benchmarks && dotnet run -c Release

# Run specific benchmark
cd benchmarks && dotnet run -c Release -- --filter "*ShortestPath*"

# Quick benchmark run
cd benchmarks && dotnet run -c Release -- --job short
```

## Architecture

### Dependency Injection

The project uses manual DI via `ServiceContainer.cs`. All services are instantiated in the constructor - no runtime DI framework.

```csharp
var services = new ServiceContainer();
var result = services.MazeGenerationFactory.GenerateMaze(settings);
```

### Key Classes

| Class | Purpose |
|-------|---------|
| `GameState` | Godot autoload singleton, holds settings and current maze |
| `ServiceContainer` | Manual DI container, instantiates all services |
| `MazeGenerationFactory` | Main entry point for maze generation |
| `MazeJumper` | Navigate through a generated maze |
| `ShortestPathSolver` | Dijkstra-based pathfinding |

### Maze Generation Flow

1. `MazeGenerationSettings` → Configuration
2. `MazeModelFactory` → Creates maze model (Model1/2/3)
3. `Algorithm.GenerateMaze()` → Carves passages
4. `DeadEndFiller` → Wraps model for dead-end hiding
5. `HeuristicsGenerator` → Calculates statistics and shortest path
6. `MazeGenerationResults` → Final output with maze and stats

### Direction System

Directions are flags-based for efficient storage:

```csharp
public enum Direction
{
    None = 0,
    Left = 1, Right = 2,
    Down = 4, Up = 8,
    Back = 16, Forward = 32
}
```

- **X axis**: Left (-X), Right (+X)
- **Y axis**: Back (-Y), Forward (+Y)
- **Z axis**: Down (-Z), Up (+Z)

### Coordinate System

In Godot 2D rendering:
- Positive Y goes **down** on screen
- Forward (Y+1 in maze) renders as going **down**
- Back (Y-1 in maze) renders as going **up**

## Test Organization

| File | Purpose |
|------|---------|
| `MazeGenerationTests.cs` | Basic maze generation, all algorithms |
| `MazeConnectivityTests.cs` | Path finding, connectivity verification |
| `AgentTests.cs` | PerfectAgent, RandomAgent behavior |
| `MazeComponentTests.cs` | MazeHelper, MazeJumper, utility classes |
| `EdgeCaseTests.cs` | Small mazes, edge cases, direction parsing |

## File Locations

- Main project: `ProceduralGeneration3DMazes.csproj`
- Tests: `tests/ProceduralMaze.Tests.csproj`
- Experiments: `experiments/ProceduralMaze.Experiments.csproj`
- Benchmarks: `benchmarks/ProceduralMaze.Benchmarks.csproj`
- Godot scenes: `scenes/*.tscn`
- Maze logic: `scripts/maze/`
- UI code: `scripts/ui/`

## Adding New Features

1. Add maze logic in `scripts/maze/` (pure C#, no Godot dependencies)
2. Wire up in `ServiceContainer.cs` if new service
3. Add tests in `tests/`
4. Add UI in `scripts/ui/` and `scenes/`

## Running the Game

```bash
# From Godot editor - open project and press F5
# Or from command line:
/path/to/godot --path .
```

## Performance Testing

The project includes BenchmarkDotNet benchmarks for performance-critical code.

### Running Benchmarks

```bash
# List all available benchmarks
cd benchmarks && dotnet run -c Release -- --list flat

# Run all benchmarks (takes several minutes)
cd benchmarks && dotnet run -c Release

# Run specific benchmark class
cd benchmarks && dotnet run -c Release -- --filter "*MazeGeneration*"

# Quick run for development
cd benchmarks && dotnet run -c Release -- --filter "*ShortestPath*" -j short

# When running benchmarks as an AI agent, pipe through tail to reduce output
cd benchmarks && dotnet run -c Release -- --filter "*ShortestPath*" -j short 2>&1 | tail -30
```

### Benchmark Categories

| File | Benchmarks |
|------|------------|
| `MazeGenerationBenchmarks.cs` | Algorithm comparison (Backtracker, GrowingTree, BinaryTree) at various sizes |
| `PathfindingBenchmarks.cs` | GraphBuilder and ShortestPathSolver at various maze sizes |
| `HelperBenchmarks.cs` | ArrayHelper.Shuffle, DirectionsFlagParser performance |

### When Making Performance Changes

**Always benchmark before and after changes:**

1. Run relevant benchmarks before making changes:
   ```bash
   cd benchmarks && dotnet run -c Release -- --filter "*YourArea*" --exporters json
   ```

2. Make your changes

3. Run the same benchmarks after:
   ```bash
   cd benchmarks && dotnet run -c Release -- --filter "*YourArea*" --exporters json
   ```

4. Compare results in `benchmarks/BenchmarkDotNet.Artifacts/`

### Key Performance-Critical Areas

- `ShortestPathSolver.ProcessGraph` - Hybrid Dijkstra: O(n²) for small graphs, PriorityQueue O(n log n) for large graphs (threshold: 1500 nodes)
- `GrowingTreeAlgorithmLinkedList` - Uses `ElementAt()` which is O(n) on LinkedList
- `DirectionsFlagParser.SplitDirectionsFromFlag` - Called frequently, allocates arrays
- Maze generation algorithms - Main user-facing performance

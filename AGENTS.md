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
| `MazeSession` | **Godot-free** session state and operations — where behaviour belongs |
| `GameState` | Thin Godot autoload adapter forwarding to `MazeSession` |
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
- Session state/behaviour (Godot-free): `scripts/session/`
- UI code: `scripts/ui/`
- Web test bridge: `scripts/testing/`

## Adding New Features

1. Add maze logic in `scripts/maze/` (pure C#, no Godot dependencies)
2. Wire up in `ServiceContainer.cs` if new service
3. Add tests in `tests/`
4. Add UI in `scripts/ui/` and `scenes/`

## Testing

Four layers, each with a different cost. **Push tests down** — see
[docs/TESTING.md](./docs/TESTING.md) for which to use.

| Layer | Command | Needs |
|---|---|---|
| Unit + integration (560 tests, ~1s) | `cd tests && dotnet test` | .NET only |
| Scene / UI (in-engine) | `dotnet build -p:IncludeSceneTests=true` then `godot --headless --path . res://tests/scene/scene_tests.tscn` | Godot binary |
| Functional (browser) | `cd tests/visual && npx playwright test --project=functional` | deployed build |
| Visual | `cd tests/visual && npx playwright test --project=maze` | deployed build |

**Keep behaviour out of Godot types.** New logic belongs in a plain C# class that the unit
suite compiles (`scripts/session/`, `scripts/maze/`); Godot `Node` subclasses should be thin
adapters that forward to it — `GameState` → `MazeSession` is the pattern. Adding a file to the
test project's `<Compile Include>` list is a claim that it is Godot-free, and the build
enforces that claim. Only genuinely engine-bound code (drawing, input, node wiring, scene
lifecycle) should need scene tests. Browser tests need the in-app test bridge
([docs/TEST_BRIDGE.md](./docs/TEST_BRIDGE.md)) because a Godot web export is a single
`<canvas>` with no DOM for Playwright to query.

## Randomness & Determinism (read before touching generation)

Maze generation is **seed-deterministic**: the same `MazeGenerationSettings.Seed` plus the
same settings always produces the same maze. Golden-file regression testing depends on it.
See [docs/REGRESSION_TESTING.md](./docs/REGRESSION_TESTING.md).

**The rule: all randomness goes through an injected `IRandomValueGenerator`.**

```csharp
// Yes — injected, seeded, reproducible
_randomValueGenerator.Shuffle(carvableDirections);
var n = _randomValueGenerator.GetNext(0, size.X - 1);   // INCLUSIVE range

// No — process-global, unseedable, silently breaks reproducibility
Random.Shared.Shuffle(directions);
var r = new Random().Next(10);
```

Banned in `scripts/maze/`: `Random.Shared`, `new Random(`, `Guid.NewGuid`,
`DateTime.Now/UtcNow` (use the injected `ISystemClock`). `RandomnessDisciplineTests`
enforces this by scanning source and will fail the build with the offending line — it is not
a style preference, it's the thing that keeps the seed meaningful.

The only exempt files are `RandomValueGenerator.cs` and `SystemClock.cs`, the designated
injected sources. Adding to that exemption list adds a global-state escape hatch.

**Reproducing a bug:** every result carries `MazeGenerationResults.Seed`, including runs
that didn't ask for a seed. Put that value in `settings.Seed` to regenerate the exact maze.

**Threading:** a generator instance is deliberately not thread-safe — per-instance state is
what makes seeding work. Give each concurrent pipeline its own `ServiceContainer`, as the
test suite does. Nothing in the maze pipeline is currently concurrent.

## Web Export Constraints (read before adding BCL dependencies)

This project ships a browser build (`maze.ryankelly.dev`) via an **experimental**
C#→WASM export on a community-patched Godot editor. See
[docs/WEB_EXPORT.md](./docs/WEB_EXPORT.md) for how it works and
[docs/WEB_EXPORT_ROADMAP.md](./docs/WEB_EXPORT_ROADMAP.md) for the plan to get onto
official support.

**The web runtime is a subset of desktop .NET. These fail *only* in the browser — the
desktop build and the whole test suite stay green:**

| Don't use | Why |
|---|---|
| Culture-sensitive formatting/parsing | `InvariantGlobalization=true` is forced; culture data is trimmed out |
| `System.Security.Cryptography` | Crypto BCL APIs are non-functional in the patched runtime |
| GDExtension / native addons | The .NET runtime is built without position-independent code |

Prefer `CultureInfo.InvariantCulture` explicitly, and keep `scripts/maze/` free of
platform-specific BCL calls. If you add anything in the table above, **test it on web
explicitly**: comment `/preview` on the PR and check the smoke test result — a green
desktop CI proves nothing about the browser build.

**Version locking:** the patched editor tag and export templates live in
`.github/web-toolchain.env`, and `Godot.NET.Sdk` in the `.csproj` must match them at
patch level. CI fails fast on drift. Never hardcode these versions into workflow files.

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

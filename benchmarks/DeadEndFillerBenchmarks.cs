using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for DeadEndFiller which fills in dead-end passages.
/// This is called during every maze generation to enable dead-end hiding.
/// 
/// Complexity: O(n * d) where n = cells, d = dead ends
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class DeadEndFillerBenchmarks
{
    private ServiceContainer _services = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();
    }

    private IMazeCarver GenerateMazeWithoutDeadEndFilling(int x, int y, int z)
    {
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Size = new MazeSize(x, y, z),
            Option = MazeType.ArrayBidirectional,
            WallRemovalPercent = 0,
            DoorsAtEdge = true
        };

        // Create model using the model factory
        var model = _services.MazeModelFactory.BuildMaze(settings);

        // Get maze carver
        var mazeCarver = _services.MazeFactory.GetMazeCarver(model);

        // Generate the maze structure (but don't fill dead ends)
        _services.RecursiveBacktrackerAlgorithm.GenerateMaze(mazeCarver, settings);

        return mazeCarver;
    }

    [Benchmark(Baseline = true)]
    public DeadEndFillerResult DeadEndFill_10x10()
    {
        // Generate fresh maze for each benchmark iteration
        var maze = GenerateMazeWithoutDeadEndFilling(10, 10, 1);
        return _services.DeadEndFiller.Fill(maze);
    }

    [Benchmark]
    public DeadEndFillerResult DeadEndFill_30x30()
    {
        var maze = GenerateMazeWithoutDeadEndFilling(30, 30, 1);
        return _services.DeadEndFiller.Fill(maze);
    }

    [Benchmark]
    public DeadEndFillerResult DeadEndFill_50x50()
    {
        var maze = GenerateMazeWithoutDeadEndFilling(50, 50, 1);
        return _services.DeadEndFiller.Fill(maze);
    }
}

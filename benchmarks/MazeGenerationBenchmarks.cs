using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Generation;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for maze generation algorithms.
/// Compares RecursiveBacktracker, GrowingTree (various strategies), and BinaryTree.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class MazeGenerationBenchmarks
{
    private ServiceContainer _services = null!;

    [Params(10, 20, 50)]
    public int MazeSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();
    }

    private MazeGenerationSettings CreateSettings(Algorithm algorithm, GrowingTreeSettings? gtSettings = null)
    {
        return new MazeGenerationSettings
        {
            Algorithm = algorithm,
            Size = new MazeSize(MazeSize, MazeSize, 1),
            Option = MazeType.ArrayBidirectional,
            WallRemovalPercent = 0,
            DoorsAtEdge = true,
            GrowingTreeSettings = gtSettings ?? new GrowingTreeSettings()
        };
    }

    [Benchmark(Baseline = true)]
    public MazeGenerationResults RecursiveBacktracker()
    {
        var settings = CreateSettings(Algorithm.RecursiveBacktrackerAlgorithm);
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }

    [Benchmark]
    public MazeGenerationResults GrowingTree_Newest()
    {
        var settings = CreateSettings(Algorithm.GrowingTreeAlgorithm, new GrowingTreeSettings
        {
            Strategies = [GrowingTreeStrategy.Newest],
            NewestWeight = 100
        });
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }

    [Benchmark]
    public MazeGenerationResults GrowingTree_Random()
    {
        var settings = CreateSettings(Algorithm.GrowingTreeAlgorithm, new GrowingTreeSettings
        {
            Strategies = [GrowingTreeStrategy.Random],
            RandomWeight = 100
        });
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }

    [Benchmark]
    public MazeGenerationResults GrowingTree_Mixed()
    {
        var settings = CreateSettings(Algorithm.GrowingTreeAlgorithm, new GrowingTreeSettings
        {
            Strategies = [GrowingTreeStrategy.Newest, GrowingTreeStrategy.Random],
            NewestWeight = 75,
            RandomWeight = 25
        });
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }

    [Benchmark]
    public MazeGenerationResults BinaryTree()
    {
        var settings = CreateSettings(Algorithm.BinaryTreeAlgorithm);
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }
}

using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Serialization;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for pathfinding algorithms.
/// Tests ShortestPathSolver (Dijkstra) and GraphBuilder performance.
/// 
/// Complexity levels (ordered by cell count):
///   L1: 10x10x1   =     100 cells (generated)
///   L2: 30x30x1   =     900 cells (generated)
///   L3: 75x75x1   =   5,625 cells (from file)
///   L4: 50x50x3   =   7,500 cells (generated)
///   L5: 40x40x20  =  32,000 cells (from file)
///   L6: 50x50x20  =  50,000 cells (from file)
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class PathfindingBenchmarks
{
    private static readonly string SampleDataDirectory = Path.Combine(
        AppContext.BaseDirectory, "sample-data");

    private ServiceContainer _services = null!;
    
    // Mazes ordered by complexity level
    private IMazeJumper _mazeL1 = null!;  // 10x10x1   =     100 cells
    private IMazeJumper _mazeL2 = null!;  // 30x30x1   =     900 cells
    private IMazeJumper _mazeL3 = null!;  // 75x75x1   =   5,625 cells
    private IMazeJumper _mazeL4 = null!;  // 50x50x3   =   7,500 cells
    private IMazeJumper _mazeL5 = null!;  // 40x40x20  =  32,000 cells
    private IMazeJumper _mazeL6 = null!;  // 50x50x20  =  50,000 cells
    
    // Pre-built graphs for graph-only benchmarks
    private Graph _graphL1 = null!;
    private Graph _graphL2 = null!;
    private Graph _graphL4 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();

        // Generate smaller mazes
        _mazeL1 = GenerateMaze(10, 10, 1);
        _mazeL2 = GenerateMaze(30, 30, 1);
        _mazeL4 = GenerateMaze(50, 50, 3);

        // Load larger mazes from files
        _mazeL3 = LoadMaze("75x75x1.maze");
        _mazeL5 = LoadMaze("40x40x20.maze");
        _mazeL6 = LoadMaze("50x50x20.maze");

        // Pre-build graphs for graph-only benchmarks
        _graphL1 = _services.GraphBuilder.GetGraphFromMaze(_mazeL1);
        _graphL2 = _services.GraphBuilder.GetGraphFromMaze(_mazeL2);
        _graphL4 = _services.GraphBuilder.GetGraphFromMaze(_mazeL4);
    }

    private IMazeJumper GenerateMaze(int x, int y, int z)
    {
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Size = new MazeSize(x, y, z),
            Option = MazeType.ArrayBidirectional,
            WallRemovalPercent = 0,
            DoorsAtEdge = true
        };
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        return result.MazeJumper;
    }

    private IMazeJumper LoadMaze(string filename)
    {
        var path = Path.Combine(SampleDataDirectory, filename);
        var content = File.ReadAllText(path);
        var model = _services.MazeDeserializer.DeserializeFromString(content);
        return _services.MazeFactory.GetMazeJumperFromModel(model);
    }

    #region GraphBuilder Benchmarks

    [Benchmark]
    public Graph GraphBuilder_L1_10x10x1()
    {
        return _services.GraphBuilder.GetGraphFromMaze(_mazeL1);
    }

    [Benchmark]
    public Graph GraphBuilder_L2_30x30x1()
    {
        return _services.GraphBuilder.GetGraphFromMaze(_mazeL2);
    }

    [Benchmark]
    public Graph GraphBuilder_L4_50x50x3()
    {
        return _services.GraphBuilder.GetGraphFromMaze(_mazeL4);
    }

    #endregion

    #region ShortestPath Benchmarks

    [Benchmark(Baseline = true)]
    public ShortestPathResult ShortestPath_L1_10x10x1()
    {
        return _services.ShortestPathSolver.GetGraph(_mazeL1);
    }

    [Benchmark]
    public ShortestPathResult ShortestPath_L2_30x30x1()
    {
        return _services.ShortestPathSolver.GetGraph(_mazeL2);
    }

    [Benchmark]
    public ShortestPathResult ShortestPath_L3_75x75x1()
    {
        return _services.ShortestPathSolver.GetGraph(_mazeL3);
    }

    [Benchmark]
    public ShortestPathResult ShortestPath_L4_50x50x3()
    {
        return _services.ShortestPathSolver.GetGraph(_mazeL4);
    }

    [Benchmark]
    public ShortestPathResult ShortestPath_L5_40x40x20()
    {
        return _services.ShortestPathSolver.GetGraph(_mazeL5);
    }

    [Benchmark]
    public ShortestPathResult ShortestPath_L6_50x50x20()
    {
        return _services.ShortestPathSolver.GetGraph(_mazeL6);
    }

    #endregion
}

using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for KShortestPathsSolver (Yen's algorithm).
/// Finds multiple alternative paths through a maze.
/// 
/// Complexity: O(k * n * m) where k = paths, n = nodes, m = edges
/// Much more expensive than single shortest path for k > 1.
/// 
/// Note: For perfect mazes (no loops), only one path exists.
/// Wall removal creates loops enabling alternative paths.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class KShortestPathsBenchmarks
{
    private ServiceContainer _services = null!;
    
    // Perfect maze (no loops, only 1 path exists)
    private IMazeJumper _perfectMaze20x20 = null!;
    
    // Mazes with loops (wall removal enables alternative paths)
    private IMazeJumper _loopMaze20x20 = null!;
    private IMazeJumper _loopMaze30x30 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();

        // Perfect maze - no wall removal
        _perfectMaze20x20 = GenerateMaze(20, 20, 1, wallRemovalPercent: 0);

        // Mazes with loops - 10% wall removal creates multiple paths
        _loopMaze20x20 = GenerateMaze(20, 20, 1, wallRemovalPercent: 10);
        _loopMaze30x30 = GenerateMaze(30, 30, 1, wallRemovalPercent: 10);
    }

    private IMazeJumper GenerateMaze(int x, int y, int z, double wallRemovalPercent)
    {
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Size = new MazeSize(x, y, z),
            Option = MazeType.ArrayBidirectional,
            WallRemovalPercent = wallRemovalPercent,
            DoorsAtEdge = true
        };
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        return result.MazeJumper;
    }

    #region Perfect Maze (baseline - only 1 path)

    [Benchmark(Baseline = true)]
    public List<PathResult> KPaths_Perfect_20x20_K1()
    {
        return _services.KShortestPathsSolver.GetKShortestPaths(_perfectMaze20x20, 1);
    }

    [Benchmark]
    public List<PathResult> KPaths_Perfect_20x20_K3()
    {
        // Should return only 1 path since perfect maze has no alternatives
        return _services.KShortestPathsSolver.GetKShortestPaths(_perfectMaze20x20, 3);
    }

    #endregion

    #region Loop Maze 20x20

    [Benchmark]
    public List<PathResult> KPaths_Loop_20x20_K1()
    {
        return _services.KShortestPathsSolver.GetKShortestPaths(_loopMaze20x20, 1);
    }

    [Benchmark]
    public List<PathResult> KPaths_Loop_20x20_K3()
    {
        return _services.KShortestPathsSolver.GetKShortestPaths(_loopMaze20x20, 3);
    }

    [Benchmark]
    public List<PathResult> KPaths_Loop_20x20_K5()
    {
        return _services.KShortestPathsSolver.GetKShortestPaths(_loopMaze20x20, 5);
    }

    #endregion

    #region Loop Maze 30x30

    [Benchmark]
    public List<PathResult> KPaths_Loop_30x30_K1()
    {
        return _services.KShortestPathsSolver.GetKShortestPaths(_loopMaze30x30, 1);
    }

    [Benchmark]
    public List<PathResult> KPaths_Loop_30x30_K3()
    {
        return _services.KShortestPathsSolver.GetKShortestPaths(_loopMaze30x30, 3);
    }

    [Benchmark]
    public List<PathResult> KPaths_Loop_30x30_K5()
    {
        return _services.KShortestPathsSolver.GetKShortestPaths(_loopMaze30x30, 5);
    }

    #endregion
}

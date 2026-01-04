using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for PointsAndDirectionsRetriever.
/// Retrieves dead ends, corridors, and junctions from a maze.
/// 
/// Used by DeadEndFiller, RandomCarver, and GraphBuilder.
/// Uses LINQ with multiple enumeration on direction counts.
/// 
/// Complexity: O(n) per call where n = cells
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class PointsAndDirectionsRetrieverBenchmarks
{
    private ServiceContainer _services = null!;
    
    // Pre-generated mazes
    private IMazeJumper _maze10x10 = null!;
    private IMazeJumper _maze30x30 = null!;
    private IMazeJumper _maze50x50 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();

        _maze10x10 = GenerateMaze(10, 10, 1);
        _maze30x30 = GenerateMaze(30, 30, 1);
        _maze50x50 = GenerateMaze(50, 50, 1);
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

    #region GetDeadEnds Benchmarks

    [Benchmark(Baseline = true)]
    public int GetDeadEnds_10x10()
    {
        return _services.PointsAndDirectionsRetriever.GetDeadEnds(_maze10x10).Count();
    }

    [Benchmark]
    public int GetDeadEnds_30x30()
    {
        return _services.PointsAndDirectionsRetriever.GetDeadEnds(_maze30x30).Count();
    }

    [Benchmark]
    public int GetDeadEnds_50x50()
    {
        return _services.PointsAndDirectionsRetriever.GetDeadEnds(_maze50x50).Count();
    }

    #endregion

    #region GetCorridors Benchmarks

    [Benchmark]
    public int GetCorridors_10x10()
    {
        return _services.PointsAndDirectionsRetriever.GetCorridoors(_maze10x10).Count();
    }

    [Benchmark]
    public int GetCorridors_30x30()
    {
        return _services.PointsAndDirectionsRetriever.GetCorridoors(_maze30x30).Count();
    }

    [Benchmark]
    public int GetCorridors_50x50()
    {
        return _services.PointsAndDirectionsRetriever.GetCorridoors(_maze50x50).Count();
    }

    #endregion

    #region GetJunctions Benchmarks

    [Benchmark]
    public int GetJunctions_10x10()
    {
        return _services.PointsAndDirectionsRetriever.GetJunctions(_maze10x10).Count();
    }

    [Benchmark]
    public int GetJunctions_30x30()
    {
        return _services.PointsAndDirectionsRetriever.GetJunctions(_maze30x30).Count();
    }

    [Benchmark]
    public int GetJunctions_50x50()
    {
        return _services.PointsAndDirectionsRetriever.GetJunctions(_maze50x50).Count();
    }

    #endregion
}

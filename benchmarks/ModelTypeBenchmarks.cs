using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks comparing different maze model types.
/// 
/// ArrayBidirectionalModel: O(1) lookup, higher memory, fast init
/// DictionaryModel: O(1) amortized lookup, variable memory, LINQ overhead on init
/// 
/// Tests both initialization and access patterns.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class ModelTypeBenchmarks
{
    private ServiceContainer _services = null!;
    
    // Pre-generated mazes of each type
    private IMazeJumper _arrayMaze30x30 = null!;
    private IMazeJumper _dictMaze30x30 = null!;
    private IMazeJumper _arrayMaze50x50 = null!;
    private IMazeJumper _dictMaze50x50 = null!;

    // Points for access benchmarks
    private MazePoint[] _accessPoints30x30 = null!;
    private MazePoint[] _accessPoints50x50 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();

        // Generate mazes with each model type
        _arrayMaze30x30 = GenerateMaze(30, 30, 1, MazeType.ArrayBidirectional);
        _dictMaze30x30 = GenerateMaze(30, 30, 1, MazeType.Dictionary);
        _arrayMaze50x50 = GenerateMaze(50, 50, 1, MazeType.ArrayBidirectional);
        _dictMaze50x50 = GenerateMaze(50, 50, 1, MazeType.Dictionary);

        // Generate random access points
        var random = new Random(42);
        _accessPoints30x30 = GenerateAccessPoints(30, 30, 100, random);
        _accessPoints50x50 = GenerateAccessPoints(50, 50, 100, random);
    }

    private IMazeJumper GenerateMaze(int x, int y, int z, MazeType mazeType)
    {
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Size = new MazeSize(x, y, z),
            Option = mazeType,
            WallRemovalPercent = 0,
            DoorsAtEdge = true
        };
        var result = _services.MazeGenerationFactory.GenerateMaze(settings);
        return result.MazeJumper;
    }

    private MazePoint[] GenerateAccessPoints(int maxX, int maxY, int count, Random random)
    {
        var points = new MazePoint[count];
        for (int i = 0; i < count; i++)
        {
            points[i] = new MazePoint(random.Next(maxX), random.Next(maxY), 0);
        }
        return points;
    }

    #region Model Initialization Benchmarks

    [Benchmark(Baseline = true)]
    public MazeGenerationResults Init_Array_30x30()
    {
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Size = new MazeSize(30, 30, 1),
            Option = MazeType.ArrayBidirectional,
            WallRemovalPercent = 0,
            DoorsAtEdge = true
        };
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }

    [Benchmark]
    public MazeGenerationResults Init_Dictionary_30x30()
    {
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Size = new MazeSize(30, 30, 1),
            Option = MazeType.Dictionary,
            WallRemovalPercent = 0,
            DoorsAtEdge = true
        };
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }

    [Benchmark]
    public MazeGenerationResults Init_Array_50x50()
    {
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Size = new MazeSize(50, 50, 1),
            Option = MazeType.ArrayBidirectional,
            WallRemovalPercent = 0,
            DoorsAtEdge = true
        };
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }

    [Benchmark]
    public MazeGenerationResults Init_Dictionary_50x50()
    {
        var settings = new MazeGenerationSettings
        {
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Size = new MazeSize(50, 50, 1),
            Option = MazeType.Dictionary,
            WallRemovalPercent = 0,
            DoorsAtEdge = true
        };
        return _services.MazeGenerationFactory.GenerateMaze(settings);
    }

    #endregion

    #region Random Access Benchmarks

    [Benchmark]
    public int Access_Array_30x30()
    {
        int sum = 0;
        foreach (var point in _accessPoints30x30)
        {
            _arrayMaze30x30.JumpToPoint(point);
            sum += _arrayMaze30x30.GetDirectionsFromPoint().Length;
        }
        return sum;
    }

    [Benchmark]
    public int Access_Dictionary_30x30()
    {
        int sum = 0;
        foreach (var point in _accessPoints30x30)
        {
            _dictMaze30x30.JumpToPoint(point);
            sum += _dictMaze30x30.GetDirectionsFromPoint().Length;
        }
        return sum;
    }

    [Benchmark]
    public int Access_Array_50x50()
    {
        int sum = 0;
        foreach (var point in _accessPoints50x50)
        {
            _arrayMaze50x50.JumpToPoint(point);
            sum += _arrayMaze50x50.GetDirectionsFromPoint().Length;
        }
        return sum;
    }

    [Benchmark]
    public int Access_Dictionary_50x50()
    {
        int sum = 0;
        foreach (var point in _accessPoints50x50)
        {
            _dictMaze50x50.JumpToPoint(point);
            sum += _dictMaze50x50.GetDirectionsFromPoint().Length;
        }
        return sum;
    }

    #endregion
}

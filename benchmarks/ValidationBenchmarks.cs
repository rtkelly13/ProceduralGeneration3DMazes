using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Serialization;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for MazeValidator which checks maze consistency.
/// Validates bidirectional connections, boundary violations, and reachability.
/// 
/// Complexity: O(n * d) where n = cells, d = avg directions per cell
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class ValidationBenchmarks
{
    private static readonly string SampleDataDirectory = Path.Combine(
        AppContext.BaseDirectory, "sample-data");

    private ServiceContainer _services = null!;
    
    // Pre-loaded models for validation
    private IModel _model10x10 = null!;
    private IModel _model30x30 = null!;
    private IModel _model75x75 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();

        // Generate smaller mazes and get their models
        _model10x10 = GenerateMazeModel(10, 10, 1);
        _model30x30 = GenerateMazeModel(30, 30, 1);

        // Load larger maze from file
        var content = File.ReadAllText(Path.Combine(SampleDataDirectory, "75x75x1.maze"));
        _model75x75 = _services.MazeDeserializer.DeserializeFromString(content);
    }

    private IModel GenerateMazeModel(int x, int y, int z)
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
        
        // Use GetModel() to get the IModel for validation
        return result.MazeJumper.GetModel();
    }

    [Benchmark(Baseline = true)]
    public MazeValidationResult Validate_10x10()
    {
        return _services.MazeValidator.Validate(_model10x10);
    }

    [Benchmark]
    public MazeValidationResult Validate_30x30()
    {
        return _services.MazeValidator.Validate(_model30x30);
    }

    [Benchmark]
    public MazeValidationResult Validate_75x75()
    {
        return _services.MazeValidator.Validate(_model75x75);
    }
}

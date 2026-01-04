using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Serialization;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for MazeDeserializer which parses saved maze files.
/// Uses span-based parsing to minimize allocations.
/// 
/// Complexity: O(n) where n = file size / number of cells
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class DeserializationBenchmarks
{
    private static readonly string SampleDataDirectory = Path.Combine(
        AppContext.BaseDirectory, "sample-data");

    private ServiceContainer _services = null!;
    
    // Pre-serialized maze content strings
    private string _maze10x10Content = null!;
    private string _maze30x30Content = null!;
    private string _maze75x75Content = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();

        // Generate and serialize smaller mazes
        _maze10x10Content = GenerateAndSerialize(10, 10, 1);
        _maze30x30Content = GenerateAndSerialize(30, 30, 1);

        // Load larger maze from file
        _maze75x75Content = File.ReadAllText(Path.Combine(SampleDataDirectory, "75x75x1.maze"));
    }

    private string GenerateAndSerialize(int x, int y, int z)
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
        
        // Use GetModel() to get the IModel for serialization
        return _services.MazeSerializer.SerializeToString(result.MazeJumper.GetModel());
    }

    [Benchmark(Baseline = true)]
    public ReadOnlyMazeModel Deserialize_10x10()
    {
        return _services.MazeDeserializer.DeserializeFromString(_maze10x10Content);
    }

    [Benchmark]
    public ReadOnlyMazeModel Deserialize_30x30()
    {
        return _services.MazeDeserializer.DeserializeFromString(_maze30x30Content);
    }

    [Benchmark]
    public ReadOnlyMazeModel Deserialize_75x75()
    {
        return _services.MazeDeserializer.DeserializeFromString(_maze75x75Content);
    }
}

using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Autoload;

namespace ProceduralMaze.Experiments;

public class ExperimentRunner : IExperimentRunner
{
    private readonly ServiceContainer _services;
    private readonly IOutputWriter _outputWriter;

    // Experiment settings configurations
    private readonly List<(string Name, MazeGenerationSettings Settings)> _experiments;

    public ExperimentRunner(ServiceContainer services, IOutputWriter outputWriter)
    {
        _services = services;
        _outputWriter = outputWriter;

        _experiments = new List<(string, MazeGenerationSettings)>
        {
            ("Binary tree with doors at edge", CreateSettings(Algorithm.BinaryTreeAlgorithm, doorsAtEdge: true)),
            ("Binary tree", CreateSettings(Algorithm.BinaryTreeAlgorithm, doorsAtEdge: false)),
            ("Recursive backtracker with doors at edge", CreateSettings(Algorithm.RecursiveBacktrackerAlgorithm, doorsAtEdge: true)),
            ("Recursive backtracker", CreateSettings(Algorithm.RecursiveBacktrackerAlgorithm, doorsAtEdge: false)),
            ("Growing Tree (Newest/Recursive) with doors at edge", CreateGrowingTreeSettings(new[] { GrowingTreeStrategy.Newest }, doorsAtEdge: true)),
            ("Growing Tree (Newest/Recursive)", CreateGrowingTreeSettings(new[] { GrowingTreeStrategy.Newest }, doorsAtEdge: false)),
            ("Growing Tree (Random/Prims) with doors at edge", CreateGrowingTreeSettings(new[] { GrowingTreeStrategy.Random }, doorsAtEdge: true)),
            ("Growing Tree (Random/Prims)", CreateGrowingTreeSettings(new[] { GrowingTreeStrategy.Random }, doorsAtEdge: false)),
            ("Growing Tree (Mixed: Newest, Oldest) with doors at edge", CreateGrowingTreeSettings(new[] { GrowingTreeStrategy.Newest, GrowingTreeStrategy.Oldest }, doorsAtEdge: true)),
            ("Growing Tree (Mixed: Newest, Oldest)", CreateGrowingTreeSettings(new[] { GrowingTreeStrategy.Newest, GrowingTreeStrategy.Oldest }, doorsAtEdge: false)),
        };
    }

    private static MazeGenerationSettings CreateSettings(Algorithm algorithm, bool doorsAtEdge)
    {
        return new MazeGenerationSettings
        {
            Algorithm = algorithm,
            AgentType = AgentType.None,
            DoorsAtEdge = doorsAtEdge,
            WallRemovalPercent = 0,
            Option = MazeType.ArrayBidirectional,
            Size = new MazeSize(25, 25, 25)
        };
    }

    private static MazeGenerationSettings CreateGrowingTreeSettings(GrowingTreeStrategy[] strategies, bool doorsAtEdge)
    {
        return new MazeGenerationSettings
        {
            Algorithm = Algorithm.GrowingTreeAlgorithm,
            AgentType = AgentType.None,
            DoorsAtEdge = doorsAtEdge,
            WallRemovalPercent = 0,
            Option = MazeType.ArrayBidirectional,
            Size = new MazeSize(25, 25, 25),
            GrowingTreeSettings = new GrowingTreeSettings
            {
                Strategies = strategies.ToList()
            }
        };
    }

    public void Run()
    {
        _outputWriter.PrintLn("=== Maze Generation Experiments ===");
        _outputWriter.PrintLn("");

        foreach (var (name, settings) in _experiments)
        {
            _outputWriter.PrintLn($"--- {name} ---");
            RunForSettings(settings, parallel: true, iterations: 100);
            _outputWriter.PrintLn("");
        }

        _outputWriter.PrintLn("=== Experiments Complete ===");
        _outputWriter.PrintLn("Press Enter to exit...");
        Console.ReadLine();
    }

    public void RunForSettings(MazeGenerationSettings settings, bool parallel = false, int iterations = 100)
    {
        // Test with different wall removal percentages
        var wallRemovalConfigs = new[] { 0, 25, 50, 100 };

        foreach (var wallRemovalPercent in wallRemovalConfigs)
        {
            settings.WallRemovalPercent = wallRemovalPercent;
            _outputWriter.Print($"Wall removal {wallRemovalPercent}%: ");
            var result = RunExperiment(settings, iterations, parallel);
            _outputWriter.PrintLn("Done");
            PrintResults(result);
        }
    }

    private void PrintResults(ExperimentResult result)
    {
        _outputWriter.PrintLn($"  Average shortest path: {result.AverageShortestPath:F2}");
        _outputWriter.PrintLn($"  Average generation time: {result.AverageGenerationTime.TotalMilliseconds:F2}ms");
        _outputWriter.PrintLn($"  Average cells filled in: {result.AverageCellsFilledIn:F2}");
        
        if (result.DirectionWeightings.Count > 0)
        {
            var weightings = result.DirectionWeightings
                .Select(x => $"{x.Key}: {x.Value:F2}%")
                .ToList();
            _outputWriter.PrintLn($"  Direction weightings: {string.Join(", ", weightings)}");
        }
    }

    private IEnumerable<MazeResults> GenerateMazes(MazeGenerationSettings settings, int iterations)
    {
        return Enumerable.Range(0, iterations).Select(_ => BuildMaze(settings));
    }

    private ParallelQuery<MazeResults> GenerateMazesParallel(MazeGenerationSettings settings, int iterations)
    {
        return Enumerable.Range(0, iterations)
            .AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .Select(_ => BuildMaze(settings));
    }

    private MazeResults BuildMaze(MazeGenerationSettings settings)
    {
        // Create a new ServiceContainer for thread safety in parallel execution
        var services = new ServiceContainer();
        var result = services.MazeGenerationFactory.GenerateMaze(settings);
        
        return new MazeResults
        {
            ShortestPath = result.HeuristicsResults.ShortestPathResult.ShortestPath,
            Stats = result.HeuristicsResults.Stats,
            ModelTime = result.ModelTime,
            GenerationTime = result.GenerationTime,
            DeadEndFillerTime = result.DeadEndFillerTime,
            AgentGenerationTime = result.AgentGenerationTime,
            HeuristicsTime = result.HeuristicsTime,
            TotalTime = result.TotalTime,
            TotalCellsFilledIn = result.DeadEndFillerResults.TotalCellsFilledIn
        };
    }

    private ExperimentResult RunExperiment(MazeGenerationSettings settings, int iterations, bool parallel)
    {
        var progressStep = Math.Max(1, iterations / 10);
        var count = 0;

        List<MazeResults> items;
        
        if (parallel)
        {
            items = GenerateMazesParallel(settings, iterations)
                .Select(x =>
                {
                    var current = Interlocked.Increment(ref count);
                    if (current % progressStep == 0)
                    {
                        _outputWriter.Print($"{current * 100 / iterations}% ");
                    }
                    return x;
                })
                .ToList();
        }
        else
        {
            items = GenerateMazes(settings, iterations)
                .Select(x =>
                {
                    count++;
                    if (count % progressStep == 0)
                    {
                        _outputWriter.Print($"{count * 100 / iterations}% ");
                    }
                    return x;
                })
                .ToList();
        }

        var avgTicks = items.Average(x => x.TotalTime.Ticks);
        
        return new ExperimentResult
        {
            AverageShortestPath = items.Average(x => x.ShortestPath),
            AverageGenerationTime = new TimeSpan((long)avgTicks),
            DirectionWeightings = _services.DirectionsFlagParser.Directions
                .ToDictionary(d => d, d => items.Average(x => x.Stats.DirectionsUsed[d])),
            AverageCellsFilledIn = items.Average(x => x.TotalCellsFilledIn)
        };
    }
}

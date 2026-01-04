using BenchmarkDotNet.Attributes;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for maze solving agents.
/// 
/// PerfectAgent: Recursive DFS, finds optimal path. O(n) worst case.
/// RandomAgent: Random walk avoiding backtracking. O(m) where m can be >> n.
/// 
/// Note: PerfectAgent uses recursion and can stack overflow on very large mazes.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class AgentBenchmarks
{
    private ServiceContainer _services = null!;
    
    // Pre-generated mazes
    private IMazeJumper _maze10x10 = null!;
    private IMazeJumper _maze20x20 = null!;
    private IMazeJumper _maze30x30 = null!;

    // Agents
    private IAgent _perfectAgent = null!;
    private IAgent _randomAgent = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceContainer();

        // Generate mazes
        _maze10x10 = GenerateMaze(10, 10, 1);
        _maze20x20 = GenerateMaze(20, 20, 1);
        _maze30x30 = GenerateMaze(30, 30, 1);

        // Create agents
        _perfectAgent = _services.AgentFactory.MakeAgent(AgentType.Perfect);
        _randomAgent = _services.AgentFactory.MakeAgent(AgentType.Random);
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

    #region PerfectAgent Benchmarks

    [Benchmark(Baseline = true)]
    public AgentResults PerfectAgent_10x10()
    {
        return _perfectAgent.RunAgent(_maze10x10);
    }

    [Benchmark]
    public AgentResults PerfectAgent_20x20()
    {
        return _perfectAgent.RunAgent(_maze20x20);
    }

    [Benchmark]
    public AgentResults PerfectAgent_30x30()
    {
        return _perfectAgent.RunAgent(_maze30x30);
    }

    #endregion

    #region RandomAgent Benchmarks

    [Benchmark]
    public AgentResults RandomAgent_10x10()
    {
        return _randomAgent.RunAgent(_maze10x10);
    }

    [Benchmark]
    public AgentResults RandomAgent_20x20()
    {
        return _randomAgent.RunAgent(_maze20x20);
    }

    [Benchmark]
    public AgentResults RandomAgent_30x30()
    {
        return _randomAgent.RunAgent(_maze30x30);
    }

    #endregion
}

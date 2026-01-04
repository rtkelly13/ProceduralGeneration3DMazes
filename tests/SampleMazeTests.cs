using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Serialization;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.Tests;

/// <summary>
/// Tests using maze files loaded from disk.
/// These tests verify that various maze operations work correctly on
/// real exported mazes of different sizes and dimensions.
/// 
/// Note: Large mazes (40x40x20, 50x50x20) are excluded to keep test times reasonable.
/// These are tested via benchmarks instead.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class SampleMazeTests
{
    private static readonly string SampleDataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "sample-data");

    /// <summary>
    /// Maximum total cells for a maze to be included in tests.
    /// Mazes larger than this are tested via benchmarks only.
    /// </summary>
    private const int MaxTestMazeCells = 5000;
    
    /// <summary>
    /// All available maze files for testing (excluding very large mazes).
    /// </summary>
    private static IEnumerable<string> AllMazeFiles()
    {
        return Directory.GetFiles(SampleDataDirectory, "*.maze")
            .Select(f => Path.GetFileName(f)!)
            .Where(f => GetCellCount(f) <= MaxTestMazeCells);
    }

    /// <summary>
    /// Calculate cell count from filename (e.g., "40x40x20.maze" = 32000).
    /// </summary>
    private static int GetCellCount(string filename)
    {
        var parts = Path.GetFileNameWithoutExtension(filename)!.Split('x');
        if (parts.Length != 3) return 0;
        return int.Parse(parts[0]) * int.Parse(parts[1]) * int.Parse(parts[2]);
    }
    
    /// <summary>
    /// Multi-level maze files (Z > 1) for 3D-specific tests.
    /// </summary>
    private static IEnumerable<string> MultiLevelMazeFiles()
    {
        return AllMazeFiles().Where(f => 
        {
            var parts = Path.GetFileNameWithoutExtension(f)!.Split('x');
            return parts.Length == 3 && int.Parse(parts[2]) > 1;
        });
    }
    
    /// <summary>
    /// Creates a new ServiceContainer for thread-safe parallel test execution.
    /// </summary>
    private static ServiceContainer CreateServices() => new ServiceContainer();
    
    private static (IModel model, IMazeJumper jumper) LoadMaze(string filename, ServiceContainer services)
    {
        var path = Path.Combine(SampleDataDirectory, filename);
        var content = File.ReadAllText(path);
        var model = services.MazeDeserializer.DeserializeFromString(content);
        var jumper = services.MazeFactory.GetMazeJumperFromModel(model);
        return (model, jumper);
    }

    #region Basic Loading Tests

    [Test]
    public void MazeFiles_Exist()
    {
        var files = AllMazeFiles().ToList();
        Assert.That(files, Is.Not.Empty, "Should have maze files in the mazes directory");
        TestContext.WriteLine($"Found {files.Count} maze files: {string.Join(", ", files)}");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void Maze_LoadsCorrectly(string filename)
    {
        var services = CreateServices();
        var (model, jumper) = LoadMaze(filename, services);
        
        Assert.That(model, Is.Not.Null);
        Assert.That(jumper, Is.Not.Null);
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void Maze_HasExpectedDimensions(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        
        // Parse expected dimensions from filename (e.g., "10x10x1.maze")
        var dims = Path.GetFileNameWithoutExtension(filename)!.Split('x');
        int expectedX = int.Parse(dims[0]);
        int expectedY = int.Parse(dims[1]);
        int expectedZ = int.Parse(dims[2]);
        
        Assert.That(jumper.Size.X, Is.EqualTo(expectedX));
        Assert.That(jumper.Size.Y, Is.EqualTo(expectedY));
        Assert.That(jumper.Size.Z, Is.EqualTo(expectedZ));
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void Maze_HasValidStartAndEnd(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        
        Assert.That(jumper.StartPoint.X, Is.InRange(0, jumper.Size.X - 1));
        Assert.That(jumper.StartPoint.Y, Is.InRange(0, jumper.Size.Y - 1));
        Assert.That(jumper.StartPoint.Z, Is.InRange(0, jumper.Size.Z - 1));
        Assert.That(jumper.EndPoint.X, Is.InRange(0, jumper.Size.X - 1));
        Assert.That(jumper.EndPoint.Y, Is.InRange(0, jumper.Size.Y - 1));
        Assert.That(jumper.EndPoint.Z, Is.InRange(0, jumper.Size.Z - 1));
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void Maze_StartAndEndAreDifferent(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        
        Assert.That(jumper.StartPoint, Is.Not.EqualTo(jumper.EndPoint));
    }

    #endregion

    #region ShortestPathSolver Tests

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void ShortestPathSolver_FindsPath(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);

        Assert.That(result.ShortestPath, Is.GreaterThan(0), "Should find a path");
        Assert.That(result.ShortestPathDirections, Is.Not.Empty, "Path should have directions");
        Assert.That(result.Graph, Is.Not.Null, "Should build a graph");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void ShortestPathSolver_PathReachesEndPoint(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);
        
        // Follow the path from start to end
        var currentPoint = jumper.StartPoint;
        foreach (var direction in result.ShortestPathDirections)
        {
            currentPoint = services.MovementHelper.Move(currentPoint, direction, jumper.Size);
        }

        Assert.That(currentPoint, Is.EqualTo(jumper.EndPoint), 
            "Following the path should reach the end point");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void ShortestPathSolver_PathLengthMatchesDirectionCount(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);

        Assert.That(result.ShortestPathDirections.Length, Is.EqualTo(result.ShortestPath),
            "Path length should equal the number of directions");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void ShortestPathSolver_GraphContainsStartAndEnd(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);

        Assert.That(result.Graph.Nodes.ContainsKey(jumper.StartPoint), 
            "Graph should contain start point");
        Assert.That(result.Graph.Nodes.ContainsKey(jumper.EndPoint), 
            "Graph should contain end point");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void ShortestPathSolver_StartNodeHasDistanceZero(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);
        var startNode = result.Graph.Nodes[jumper.StartPoint];

        Assert.That(startNode.ShortestPath, Is.EqualTo(0), 
            "Start node should have distance 0");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void ShortestPathSolver_EndNodeHasCorrectDistance(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);
        var endNode = result.Graph.Nodes[jumper.EndPoint];

        Assert.That(endNode.ShortestPath, Is.EqualTo(result.ShortestPath),
            "End node distance should equal the shortest path length");
    }

    #endregion

    #region KShortestPathsSolver Tests

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void KShortestPathsSolver_FindsAtLeastOnePath(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        var paths = services.KShortestPathsSolver.GetKShortestPaths(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint, 5);

        Assert.That(paths, Is.Not.Empty, "Should find at least one path");
        Assert.That(paths[0].JunctionNodes, Is.Not.Empty, "First path should have junction nodes");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void KShortestPathsSolver_FirstPathIsOptimal(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        var paths = services.KShortestPathsSolver.GetKShortestPaths(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint, 5);

        Assert.That(paths[0].DistanceFromOptimal, Is.EqualTo(0),
            "First path should be optimal (distance from optimal = 0)");
        Assert.That(paths[0].TotalDistance, Is.EqualTo(graphResult.ShortestPath),
            "First path total distance should match shortest path");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void KShortestPathsSolver_PathsAreOrderedByLength(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        var paths = services.KShortestPathsSolver.GetKShortestPaths(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint, 10);

        // Paths should be in non-decreasing order
        for (int i = 1; i < paths.Count; i++)
        {
            Assert.That(paths[i].TotalDistance, Is.GreaterThanOrEqualTo(paths[i - 1].TotalDistance),
                $"Path {i} should be at least as long as path {i - 1}");
        }
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void KShortestPathsSolver_AllPathsStartAndEnd(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        var paths = services.KShortestPathsSolver.GetKShortestPaths(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint, 5);

        foreach (var path in paths)
        {
            Assert.That(path.JunctionNodes.First(), Is.EqualTo(jumper.StartPoint),
                "All paths should start at the start point");
            Assert.That(path.JunctionNodes.Last(), Is.EqualTo(jumper.EndPoint),
                "All paths should end at the end point");
        }
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void KShortestPathsSolver_DoesNotMutateGraph(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        // Store original distances from all nodes
        var originalDistances = new Dictionary<MazePoint, int>();
        foreach (var kvp in graphResult.Graph.Nodes)
        {
            originalDistances[kvp.Key] = kvp.Value.ShortestPath;
        }
        
        // Run K-shortest paths solver (this previously mutated the graph)
        var paths = services.KShortestPathsSolver.GetKShortestPaths(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint, 10);
        
        // Verify all distances are unchanged
        foreach (var kvp in graphResult.Graph.Nodes)
        {
            Assert.That(kvp.Value.ShortestPath, Is.EqualTo(originalDistances[kvp.Key]),
                $"Node at {kvp.Key} should not have its ShortestPath mutated");
        }
    }

    #endregion

    #region DijkstraAnimator Tests

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void DijkstraAnimator_CapturesSteps(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        var steps = services.DijkstraAnimator.CaptureSteps(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint);

        Assert.That(steps, Is.Not.Empty, "Should capture animation steps");
        Assert.That(steps.Count, Is.GreaterThan(1), "Should have multiple steps");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void DijkstraAnimator_FirstStepStartsAtStart(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        var steps = services.DijkstraAnimator.CaptureSteps(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint);

        var firstStep = steps[0];
        Assert.That(firstStep.CurrentNode, Is.EqualTo(jumper.StartPoint),
            "First step should be at start point");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void DijkstraAnimator_VisitsEndNode(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        var steps = services.DijkstraAnimator.CaptureSteps(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint);

        var endVisited = steps.Any(s => s.VisitedNodes.Contains(jumper.EndPoint));
        Assert.That(endVisited, Is.True, "End point should be visited at some step");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void DijkstraAnimator_VisitedNodesGrowMonotonically(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var graphResult = services.ShortestPathSolver.GetGraph(jumper);
        
        var steps = services.DijkstraAnimator.CaptureSteps(
            graphResult.Graph, jumper.StartPoint, jumper.EndPoint);

        int previousCount = 0;
        foreach (var step in steps)
        {
            Assert.That(step.VisitedNodes.Count, Is.GreaterThanOrEqualTo(previousCount),
                "Visited node count should never decrease");
            previousCount = step.VisitedNodes.Count;
        }
    }

    #endregion

    #region Agent Tests

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void PerfectAgent_SolvesMaze(string filename)
    {
        var services = CreateServices();
        var (model, _) = LoadMaze(filename, services);
        var jumper = services.MazeFactory.GetMazeJumperFromModel(model);
        var agent = services.AgentFactory.MakeAgent(AgentType.Perfect);

        var result = agent.RunAgent(jumper);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Movements, Is.Not.Empty, "Agent should make movements");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void PerfectAgent_PathReachesEnd(string filename)
    {
        var services = CreateServices();
        var (model, _) = LoadMaze(filename, services);
        var jumper = services.MazeFactory.GetMazeJumperFromModel(model);
        var agent = services.AgentFactory.MakeAgent(AgentType.Perfect);

        var result = agent.RunAgent(jumper);

        // Follow the movements from start
        var currentPoint = jumper.StartPoint;
        foreach (var movement in result.Movements)
        {
            currentPoint = services.MovementHelper.Move(currentPoint, movement.Direction, jumper.Size);
        }

        Assert.That(currentPoint, Is.EqualTo(jumper.EndPoint),
            "Perfect agent path should reach the end point");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void RandomAgent_SolvesMaze(string filename)
    {
        var services = CreateServices();
        var (model, _) = LoadMaze(filename, services);
        var jumper = services.MazeFactory.GetMazeJumperFromModel(model);
        var agent = services.AgentFactory.MakeAgent(AgentType.Random);

        var result = agent.RunAgent(jumper);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Movements, Is.Not.Empty, "Agent should make movements");
    }

    #endregion

    #region Dead End Filling Tests

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void DeadEndFilling_Works(string filename)
    {
        var services = CreateServices();
        var (model, _) = LoadMaze(filename, services);
        var jumper = services.MazeFactory.GetMazeJumperFromModel(model);

        jumper.SetState(ModelMode.DeadEndFilled);

        jumper.JumpToPoint(jumper.StartPoint);
        Assert.That(jumper.GetFlagFromPoint(), Is.Not.EqualTo(Direction.None),
            "Start point should still have passages after dead-end filling");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void DeadEndFilling_PathStillReachesEnd(string filename)
    {
        var services = CreateServices();
        var (model, _) = LoadMaze(filename, services);
        var jumper = services.MazeFactory.GetMazeJumperFromModel(model);
        jumper.SetState(ModelMode.DeadEndFilled);

        var result = services.ShortestPathSolver.GetGraph(jumper);

        Assert.That(result.ShortestPath, Is.GreaterThan(0), 
            "Should still find a path after dead-end filling");

        // Follow the path
        var currentPoint = jumper.StartPoint;
        foreach (var direction in result.ShortestPathDirections)
        {
            currentPoint = services.MovementHelper.Move(currentPoint, direction, jumper.Size);
        }
        Assert.That(currentPoint, Is.EqualTo(jumper.EndPoint),
            "Path should still reach end after dead-end filling");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void DeadEndFilling_SamePathLengthAsOriginal(string filename)
    {
        var services = CreateServices();
        var (model, _) = LoadMaze(filename, services);
        
        // Get original path
        var originalJumper = services.MazeFactory.GetMazeJumperFromModel(model);
        var originalResult = services.ShortestPathSolver.GetGraph(originalJumper);
        
        // Get filled path
        var filledJumper = services.MazeFactory.GetMazeJumperFromModel(model);
        filledJumper.SetState(ModelMode.DeadEndFilled);
        var filledResult = services.ShortestPathSolver.GetGraph(filledJumper);

        Assert.That(filledResult.ShortestPath, Is.EqualTo(originalResult.ShortestPath),
            "Shortest path should be the same after dead-end filling");
    }

    #endregion

    #region Serialization Round-Trip Tests

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void RoundTrip_PreservesAllData(string filename)
    {
        var services = CreateServices();
        var (originalModel, _) = LoadMaze(filename, services);

        var serialized = services.MazeSerializer.SerializeToString(originalModel);
        var reimportedModel = services.MazeDeserializer.DeserializeFromString(serialized);

        Assert.That(reimportedModel.Size.X, Is.EqualTo(originalModel.Size.X));
        Assert.That(reimportedModel.Size.Y, Is.EqualTo(originalModel.Size.Y));
        Assert.That(reimportedModel.Size.Z, Is.EqualTo(originalModel.Size.Z));
        Assert.That(reimportedModel.StartPoint, Is.EqualTo(originalModel.StartPoint));
        Assert.That(reimportedModel.EndPoint, Is.EqualTo(originalModel.EndPoint));
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void RoundTrip_PreservesAllCellConnections(string filename)
    {
        var services = CreateServices();
        var (originalModel, _) = LoadMaze(filename, services);
        var serialized = services.MazeSerializer.SerializeToString(originalModel);
        var reimportedModel = services.MazeDeserializer.DeserializeFromString(serialized);

        for (int z = 0; z < originalModel.Size.Z; z++)
        {
            for (int y = 0; y < originalModel.Size.Y; y++)
            {
                for (int x = 0; x < originalModel.Size.X; x++)
                {
                    var point = new MazePoint(x, y, z);
                    var originalDirs = originalModel.GetFlagFromPoint(point);
                    var reimportedDirs = reimportedModel.GetFlagFromPoint(point);
                    Assert.That(reimportedDirs, Is.EqualTo(originalDirs),
                        $"Cell ({x}, {y}, {z}) mismatch after round-trip");
                }
            }
        }
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void RoundTrip_SolverProducesSameResult(string filename)
    {
        var services = CreateServices();
        var (originalModel, originalJumper) = LoadMaze(filename, services);
        var originalResult = services.ShortestPathSolver.GetGraph(originalJumper);
        
        var serialized = services.MazeSerializer.SerializeToString(originalModel);
        var reimportedModel = services.MazeDeserializer.DeserializeFromString(serialized);
        var reimportedJumper = services.MazeFactory.GetMazeJumperFromModel(reimportedModel);
        var reimportedResult = services.ShortestPathSolver.GetGraph(reimportedJumper);

        Assert.That(reimportedResult.ShortestPath, Is.EqualTo(originalResult.ShortestPath),
            "Path length should be identical after round-trip");
        Assert.That(reimportedResult.ShortestPathDirections, Is.EqualTo(originalResult.ShortestPathDirections),
            "Path directions should be identical after round-trip");
    }

    #endregion

    #region Graph Structure Tests

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void Graph_HasReasonableNumberOfNodes(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);
        int nodeCount = result.Graph.Nodes.Count;

        int totalCells = jumper.Size.X * jumper.Size.Y * jumper.Size.Z;
        Assert.That(nodeCount, Is.LessThan(totalCells),
            "Graph should have fewer nodes than total cells (due to compression)");
        Assert.That(nodeCount, Is.GreaterThanOrEqualTo(2),
            "Graph should have at least start and end nodes");
    }

    [Test, TestCaseSource(nameof(AllMazeFiles))]
    public void Graph_AllNodesAreReachable(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);

        foreach (var kvp in result.Graph.Nodes)
        {
            Assert.That(kvp.Value.ShortestPath, Is.LessThan(int.MaxValue),
                $"Node at {kvp.Key} should be reachable");
        }
    }

    [Test, TestCaseSource(nameof(MultiLevelMazeFiles))]
    public void Graph_ContainsVerticalConnections(string filename)
    {
        var services = CreateServices();
        var (_, jumper) = LoadMaze(filename, services);
        var result = services.ShortestPathSolver.GetGraph(jumper);

        bool hasVerticalEdge = false;
        foreach (var kvp in result.Graph.Nodes)
        {
            foreach (var edge in kvp.Value.Edges)
            {
                if (edge.DirectionsToPoint.Any(d => d == Direction.Up || d == Direction.Down))
                {
                    hasVerticalEdge = true;
                    break;
                }
            }
            if (hasVerticalEdge) break;
        }

        Assert.That(hasVerticalEdge, Is.True,
            "3D maze should have vertical connections between levels");
    }

    #endregion
}

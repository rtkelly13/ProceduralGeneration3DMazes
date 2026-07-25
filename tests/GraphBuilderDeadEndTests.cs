using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.Tests;

/// <summary>
/// Regression tests for a crash in <c>GraphBuilder.GetGraphEdges</c>:
/// <c>System.InvalidOperationException : Nullable object must have a value</c>.
///
/// The corridor walk in <c>GetGraphEdges</c> steps from a junction until it reaches a
/// start/end point or another junction. A **dead end** has exactly one direction — the one you
/// arrived from — so it is not a junction (which needs more than two) and the walk had no
/// onward direction to take. It dereferenced a null <c>Direction?</c> instead.
///
/// Only reachable once dead-end wrapping is active, because hiding a dead-end passage turns
/// the cell before it into a new dead end. <c>GameState.LoadImportedMaze</c> wraps and then
/// builds the graph, so **importing any maze crashed the app**.
///
/// Found by extracting <c>MazeSession</c> out of the <c>GameState</c> autoload: the import flow
/// became testable without the engine, and failed on the first run. Before that, no test
/// combined wrapping with graph building — several did each separately, which is why it
/// survived.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class GraphBuilderDeadEndTests
{
    private static (ServiceContainer services, MazeGenerationResults maze) Generate(
        int x = 8, int y = 8, int z = 1, int seed = 20260725)
    {
        var services = new ServiceContainer();
        var maze = services.MazeGenerationFactory.GenerateMaze(new MazeGenerationSettings
        {
            Seed = seed,
            Size = new MazeSize { X = x, Y = y, Z = z },
            Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
            Option = MazeType.ArrayBidirectional,
        });
        return (services, maze);
    }

    private static IModel RoundTrip(ServiceContainer services, IModel model)
    {
        var text = services.MazeSerializer.SerializeToString(model);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
        return services.MazeDeserializer.Deserialize(stream);
    }

    [Test]
    public void GetGraph_OnAPlainMaze_Works()
    {
        // Control: this always passed, which is why the bug hid for so long.
        var (services, maze) = Generate();

        Assert.DoesNotThrow(() => services.ShortestPathSolver.GetGraph(maze.MazeJumper));
    }

    [Test]
    public void GetGraph_AfterDeadEndWrapping_OnAGeneratedMaze_DoesNotThrow()
    {
        var (services, maze) = Generate();
        var jumper = maze.MazeJumper;
        jumper.DoDeadEndWrapping(mb => services.DeadEndModelWrapperFactory.MakeModel(mb));

        Assert.DoesNotThrow(() => services.ShortestPathSolver.GetGraph(jumper),
            "wrapping then graph-building is the order LoadImportedMaze uses");
    }

    [Test]
    public void GetGraph_AfterDeadEndWrapping_OnAnImportedMaze_DoesNotThrow()
    {
        var (services, maze) = Generate();
        var imported = RoundTrip(services, maze.MazeJumper.GetModel());
        var jumper = services.MazeFactory.GetMazeJumperFromModel(imported);
        jumper.DoDeadEndWrapping(mb => services.DeadEndModelWrapperFactory.MakeModel(mb));

        Assert.DoesNotThrow(() => services.ShortestPathSolver.GetGraph(jumper),
            "this is exactly what happens when a user imports a .maze file");
    }

    [Test]
    public void GraphAfterWrapping_StillSolvesFromStartToEnd()
    {
        // The fix drops edges that lead into a dead end. This checks it didn't drop so much
        // that the maze became unsolvable — a silently-wrong graph would be worse than a crash.
        var (services, maze) = Generate(x: 12, y: 12);
        var jumper = maze.MazeJumper;
        jumper.DoDeadEndWrapping(mb => services.DeadEndModelWrapperFactory.MakeModel(mb));

        var result = services.ShortestPathSolver.GetGraph(jumper);

        Assert.That(result.ShortestPath, Is.GreaterThan(0),
            "a solvable maze must still report a path after wrapping");
    }

    [Test]
    public void EveryEdgeTargetsAKnownNode()
    {
        // The invariant that dictated the fix: consumers do graph.Nodes[edge.Point], a direct
        // dictionary lookup, so an edge pointing at a non-node throws KeyNotFoundException.
        // Pointing dropped edges at the dead-end cell would have traded one crash for another.
        var (services, maze) = Generate(x: 10, y: 10);
        var jumper = maze.MazeJumper;
        jumper.DoDeadEndWrapping(mb => services.DeadEndModelWrapperFactory.MakeModel(mb));

        var graph = services.ShortestPathSolver.GetGraph(jumper).Graph;

        foreach (var (point, node) in graph.Nodes)
        {
            foreach (var edge in node.Edges)
            {
                Assert.That(graph.Nodes.ContainsKey(edge.Point), Is.True,
                    $"node {point} has an edge to {edge.Point}, which is not in the graph");
            }
        }
    }

    [Test]
    public void WrappingThenGraphBuilding_IsStableAcrossSeeds()
    {
        // Dead-end topology varies with the seed, so one maze proves little. Now that seeding
        // is deterministic, a failure here names the seed that broke it.
        for (var seed = 1; seed <= 25; seed++)
        {
            var (services, maze) = Generate(x: 9, y: 9, seed: seed);
            var jumper = maze.MazeJumper;
            jumper.DoDeadEndWrapping(mb => services.DeadEndModelWrapperFactory.MakeModel(mb));

            Assert.DoesNotThrow(() => services.ShortestPathSolver.GetGraph(jumper),
                $"seed {seed} threw while building a graph after dead-end wrapping");
        }
    }

    [Test]
    public void WrappingThenGraphBuilding_WorksFor3DMazes()
    {
        var (services, maze) = Generate(x: 6, y: 6, z: 3);
        var jumper = maze.MazeJumper;
        jumper.DoDeadEndWrapping(mb => services.DeadEndModelWrapperFactory.MakeModel(mb));

        Assert.DoesNotThrow(() => services.ShortestPathSolver.GetGraph(jumper));
    }
}

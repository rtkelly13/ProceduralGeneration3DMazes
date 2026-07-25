using NUnit.Framework;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.Maze.Solver.Heuristics;
using ProceduralMaze.Session;

namespace ProceduralMaze.Tests;

/// <summary>
/// Regression tests for an **infinite loop** generating a 1×1×1 maze.
///
/// <c>MazeModelFactory.BuildMaze</c> picked an end point with
/// <c>while (start.Equals(end)) end = RandomPoint(...)</c>. A single-cell maze has exactly one
/// point, so a distinct end point could never be drawn and generation spun forever — a hang,
/// not a crash, so it produced no stack trace and no error.
///
/// Reachable by importing a `.maze` file with `SIZE 1 1 1`, which the format explicitly permits
/// ("all values must be positive integers").
///
/// Found by an exploratory sweep over sizes and option combinations, the same technique that
/// surfaced the GraphBuilder dead-end crash. Every neighbouring size (1×2×1, 2×1×1, 1×1×2,
/// 2×2×1) was already fine, which is why nothing caught it.
///
/// Each test carries a timeout: a hang regression would otherwise stall CI for the whole job
/// rather than failing this fixture.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class DegenerateMazeSizeTests
{
    private static MazeSession Session(int x, int y, int z, bool doorsAtEdge = true,
        AgentType agent = AgentType.None, int seed = 1)
    {
        var session = new MazeSession();
        session.Settings.Seed = seed;
        session.Settings.Size = new MazeSize { X = x, Y = y, Z = z };
        session.Settings.Algorithm = Algorithm.RecursiveBacktrackerAlgorithm;
        session.Settings.Option = MazeType.ArrayBidirectional;
        session.Settings.DoorsAtEdge = doorsAtEdge;
        session.Settings.AgentType = agent;
        session.Settings.SolverType = SolverType.Dijkstra;
        session.Settings.HeuristicType = HeuristicType.Manhattan;
        return session;
    }

    [Test, Timeout(30_000)]
    public void SingleCellMaze_Generates_WithoutHanging()
    {
        var session = Session(1, 1, 1);

        var result = session.GenerateMaze();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HeuristicsResults.TotalCells, Is.EqualTo(1));
            // Degenerate but finite: with one cell there is no distinct end point, so start and
            // end coincide. Callers already tolerate this (agents check before moving).
            Assert.That(result.MazeJumper.StartPoint, Is.EqualTo(result.MazeJumper.EndPoint));
        });
    }

    [Test, Timeout(30_000)]
    public void SingleCellMaze_Generates_WithEitherDoorPlacement()
    {
        // DoorsAtEdge switches the pick to PickType.RandomEdge, a different code path through
        // RandomPointGenerator, so both need covering.
        Assert.DoesNotThrow(() => Session(1, 1, 1, doorsAtEdge: false).GenerateMaze());
        Assert.DoesNotThrow(() => Session(1, 1, 1, doorsAtEdge: true).GenerateMaze());
    }

    [Test, Timeout(60_000)]
    public void SingleCellMaze_Generates_WithAnyAgent()
    {
        // An agent on a start==end maze must terminate immediately rather than walk forever.
        foreach (var agent in new[] { AgentType.None, AgentType.Random, AgentType.Perfect })
        {
            Assert.DoesNotThrow(() => Session(1, 1, 1, agent: agent).GenerateMaze(),
                $"agent {agent} on a single-cell maze");
        }
    }

    [Test, Timeout(60_000)]
    public void SingleCellMaze_SurvivesAnExportImportRoundTrip()
    {
        // The realistic route in: a hand-written or exported `SIZE 1 1 1` file.
        var session = Session(1, 1, 1);
        var generated = session.GenerateMaze();
        var text = session.Services.MazeSerializer.SerializeToString(generated.MazeJumper.GetModel());

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
        var model = session.Services.MazeDeserializer.Deserialize(stream);

        Assert.DoesNotThrow(() => session.LoadImportedMaze(model),
            "importing a single-cell maze must not hang or throw");
    }

    [Test, Timeout(120_000)]
    public void AllTinySizes_GenerateAcrossAlgorithmsAndModels()
    {
        // The neighbourhood around the bug. Sizes with a degenerate axis are where off-by-one
        // and "retry until different" assumptions break.
        var sizes = new[] { (1, 1, 1), (1, 2, 1), (2, 1, 1), (1, 1, 2), (2, 2, 1), (1, 3, 1), (3, 1, 1), (1, 1, 3) };
        var algorithms = Enum.GetValues<Algorithm>().Where(a => a != Algorithm.None);
        var models = Enum.GetValues<MazeType>().Where(m => m != MazeType.None);

        foreach (var algorithm in algorithms)
        foreach (var model in models)
        foreach (var (x, y, z) in sizes)
        {
            var session = Session(x, y, z);
            session.Settings.Algorithm = algorithm;
            session.Settings.Option = model;

            Assert.DoesNotThrow(() => session.GenerateMaze(),
                $"{algorithm} / {model} / {x}x{y}x{z}");
        }
    }

    [Test, Timeout(30_000)]
    public void TwoCellMaze_StillGetsDistinctEndpoints()
    {
        // The guard must not over-apply: as soon as two cells exist, start and end must differ.
        foreach (var (x, y, z) in new[] { (2, 1, 1), (1, 2, 1), (1, 1, 2) })
        {
            var result = Session(x, y, z).GenerateMaze();
            Assert.That(result.MazeJumper.StartPoint, Is.Not.EqualTo(result.MazeJumper.EndPoint),
                $"a {x}x{y}x{z} maze has two cells, so the endpoints should differ");
        }
    }
}

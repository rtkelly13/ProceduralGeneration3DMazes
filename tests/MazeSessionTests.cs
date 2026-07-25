using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.Maze.Solver.Heuristics;
using ProceduralMaze.Session;
using ProceduralMaze.UI;

namespace ProceduralMaze.Tests;

/// <summary>
/// Integration tests for <see cref="MazeSession"/> — whole application flows exercised
/// in-process, with no Godot engine.
///
/// These were impossible before the session was extracted from the <c>GameState</c> autoload:
/// the state and the logic lived on a Godot <c>Node</c>, so reaching them meant booting the
/// engine. `GameState` is now a thin adapter that forwards here, which means these tests drive
/// the same code paths the running app does. See docs/TESTING.md.
///
/// Unlike the unit tests around individual algorithms, these cover *sequences*: generate then
/// navigate, import then reset, switch paths then regenerate.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class MazeSessionTests
{
    private static MazeSession NewSession(int? seed = 20260725, int x = 10, int y = 10, int z = 1,
        Algorithm algorithm = Algorithm.RecursiveBacktrackerAlgorithm)
    {
        var session = new MazeSession();
        session.Settings.Seed = seed;
        session.Settings.Size = new MazeSize { X = x, Y = y, Z = z };
        session.Settings.Algorithm = algorithm;
        session.Settings.Option = MazeType.ArrayBidirectional;
        session.Settings.SolverType = SolverType.Dijkstra;
        session.Settings.HeuristicType = HeuristicType.Manhattan;
        session.Settings.AgentType = AgentType.None;
        return session;
    }

    #region Defaults and construction

    [Test]
    public void NewSession_HasSensibleDefaults()
    {
        var session = new MazeSession();

        Assert.Multiple(() =>
        {
            Assert.That(session.CurrentMaze, Is.Null, "no maze until one is generated");
            Assert.That(session.CurrentLevel, Is.Zero);
            Assert.That(session.AllPaths, Is.Empty);
            Assert.That(session.CurrentPath, Is.Null);
            Assert.That(session.IsAnimationMode, Is.False);
            Assert.That(session.Services, Is.Not.Null, "services must be ready to use");
            Assert.That(session.Settings.Size.X, Is.EqualTo(20));
            Assert.That(session.GraphLayout, Is.EqualTo(GraphLayoutType.GridAware));
        });
    }

    [Test]
    public void Session_AcceptsAnInjectedServiceContainer()
    {
        // The injection point is what lets a test pin generation without touching globals.
        var services = new ServiceContainer();
        var session = new MazeSession(services);
        Assert.That(session.Services, Is.SameAs(services));
    }

    #endregion

    #region Generation flow

    [Test]
    public void GenerateMaze_ProducesAMazeAndResetsTheLevel()
    {
        var session = NewSession(z: 3);
        session.CurrentLevel = 2;

        var result = session.GenerateMaze();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(session.CurrentMaze, Is.SameAs(result), "the session should hold the result");
            Assert.That(session.CurrentLevel, Is.Zero, "a new maze should return to level 0");
            Assert.That(result.Seed, Is.EqualTo(20260725), "the seed used must be reported");
        });
    }

    [Test]
    public void GenerateMaze_IsDeterministicThroughTheSession()
    {
        // The determinism guarantee holds through the session path the app uses, not just
        // through MazeGenerationFactory directly.
        var a = NewSession();
        var b = NewSession();

        var first = a.GenerateMaze();
        var second = b.GenerateMaze();

        var fa = a.Services.MazeSerializer.SerializeToString(first.MazeJumper.GetModel());
        var fb = b.Services.MazeSerializer.SerializeToString(second.MazeJumper.GetModel());
        Assert.That(fb, Is.EqualTo(fa));
    }

    [Test]
    public void RegeneratingWithADifferentSeed_ReplacesTheMaze()
    {
        var session = NewSession(seed: 1);
        var first = session.GenerateMaze();
        var firstFingerprint = session.Services.MazeSerializer.SerializeToString(first.MazeJumper.GetModel());

        session.Settings.Seed = 2;
        var second = session.GenerateMaze();
        var secondFingerprint = session.Services.MazeSerializer.SerializeToString(second.MazeJumper.GetModel());

        Assert.Multiple(() =>
        {
            Assert.That(second.Seed, Is.EqualTo(2));
            Assert.That(secondFingerprint, Is.Not.EqualTo(firstFingerprint));
            Assert.That(session.CurrentMaze, Is.SameAs(second));
        });
    }

    [Test]
    public void GeneratedMaze_IsSolvableWithDistinctEndpoints()
    {
        var session = NewSession(x: 12, y: 12);
        var result = session.GenerateMaze();

        Assert.Multiple(() =>
        {
            Assert.That(result.HeuristicsResults.TotalCells, Is.EqualTo(144));
            Assert.That(result.HeuristicsResults.ShortestPathResult.ShortestPath, Is.GreaterThan(0));
            Assert.That(result.MazeJumper.StartPoint, Is.Not.EqualTo(result.MazeJumper.EndPoint));
        });
    }

    #endregion

    #region Level navigation

    [Test]
    public void SetLevel_ClampsToTheMazeDepth()
    {
        var session = NewSession(z: 3);
        session.GenerateMaze();

        session.SetLevel(99);
        Assert.That(session.CurrentLevel, Is.EqualTo(2), "Z=3 means the top index is 2");

        session.SetLevel(-5);
        Assert.That(session.CurrentLevel, Is.Zero);
    }

    [Test]
    public void SetLevel_IsIgnoredWithoutAMaze()
    {
        // Guards against a level being set for a maze that doesn't exist, which would then be
        // applied to whatever is generated next.
        var session = NewSession(z: 3);

        session.SetLevel(2);

        Assert.That(session.CurrentLevel, Is.Zero);
    }

    [Test]
    public void NextAndPreviousLevel_WalkWithinBounds()
    {
        var session = NewSession(x: 5, y: 5, z: 3);
        session.GenerateMaze();

        session.NextLevel();
        Assert.That(session.CurrentLevel, Is.EqualTo(1));
        session.NextLevel();
        Assert.That(session.CurrentLevel, Is.EqualTo(2));
        session.NextLevel();
        Assert.That(session.CurrentLevel, Is.EqualTo(2), "must not exceed the top level");

        session.PreviousLevel();
        Assert.That(session.CurrentLevel, Is.EqualTo(1));
        session.PreviousLevel();
        session.PreviousLevel();
        Assert.That(session.CurrentLevel, Is.Zero, "must not go below level 0");
    }

    [Test]
    public void A2DMaze_HasOnlyOneLevel()
    {
        var session = NewSession(z: 1);
        session.GenerateMaze();

        session.NextLevel();

        Assert.That(session.CurrentLevel, Is.Zero);
    }

    #endregion

    #region Alternative path navigation

    private static List<PathResult> FakePaths(int count) =>
        Enumerable.Range(0, count).Select(i => new PathResult { PathIndex = i, TotalDistance = 10 + i }).ToList();

    [Test]
    public void PathCycling_WrapsInBothDirections()
    {
        var session = NewSession();
        session.AllPaths = FakePaths(3);

        session.NextPath();
        Assert.That(session.CurrentPathIndex, Is.EqualTo(1));
        session.NextPath();
        session.NextPath();
        Assert.That(session.CurrentPathIndex, Is.Zero, "should wrap forward to the start");

        session.PreviousPath();
        Assert.That(session.CurrentPathIndex, Is.EqualTo(2), "should wrap backward to the end");
    }

    [Test]
    public void PathCycling_IsANoOpWithFewerThanTwoPaths()
    {
        var session = NewSession();
        session.AllPaths = FakePaths(1);

        session.NextPath();
        session.PreviousPath();

        Assert.That(session.CurrentPathIndex, Is.Zero);
    }

    [Test]
    public void CurrentPath_TracksTheSelectedIndex()
    {
        var session = NewSession();
        session.AllPaths = FakePaths(3);

        session.NextPath();

        Assert.That(session.CurrentPath?.PathIndex, Is.EqualTo(1));
    }

    #endregion

    #region Reset behaviour

    [Test]
    public void ResetVisualizationState_ClearsPathAndAnimationState()
    {
        var session = NewSession();
        session.AllPaths = FakePaths(3);
        session.CurrentPathIndex = 2;
        session.AlternativePathsComputed = true;
        session.IsAnimationMode = true;
        session.AnimationSteps = new List<AlgorithmStep> { new() { Description = "s" } };
        session.AnimationController = new AnimationController(session.AnimationSteps);

        session.ResetVisualizationState();

        Assert.Multiple(() =>
        {
            Assert.That(session.AllPaths, Is.Empty);
            Assert.That(session.CurrentPathIndex, Is.Zero);
            Assert.That(session.AlternativePathsComputed, Is.False);
            Assert.That(session.IsAnimationMode, Is.False);
            Assert.That(session.AnimationSteps, Is.Null);
            Assert.That(session.AnimationController, Is.Null);
        });
    }

    #endregion

    #region Import flow

    /// <summary>Round-trips a generated maze through the serializer to get a real model.</summary>
    private static (MazeSession session, IModel model) GenerateThenDeserialize()
    {
        var source = NewSession(x: 8, y: 8, z: 1);
        var generated = source.GenerateMaze();
        var text = source.Services.MazeSerializer.SerializeToString(generated.MazeJumper.GetModel());

        var target = NewSession();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
        var model = target.Services.MazeDeserializer.Deserialize(stream);
        return (target, model);
    }

    [Test]
    public void LoadImportedMaze_AdoptsTheModelAndComputesAPath()
    {
        var (session, model) = GenerateThenDeserialize();

        var result = session.LoadImportedMaze(model);

        Assert.Multiple(() =>
        {
            Assert.That(session.CurrentMaze, Is.SameAs(result));
            Assert.That(session.Settings.Size.X, Is.EqualTo(8), "settings should adopt the imported size");
            Assert.That(session.Settings.Size.Y, Is.EqualTo(8));
            Assert.That(result.HeuristicsResults.TotalCells, Is.EqualTo(64));
            Assert.That(result.HeuristicsResults.ShortestPathResult, Is.Not.Null,
                "an imported maze still needs a solvable path computed");
            Assert.That(session.CurrentLevel, Is.Zero);
        });
    }

    [Test]
    public void LoadImportedMaze_ClearsStateLeftOverFromThePreviousMaze()
    {
        // The bug this guards: importing while alternative paths from the *previous* maze are
        // still selected, leaving the UI pointing at a path that no longer exists.
        var (session, model) = GenerateThenDeserialize();
        session.GenerateMaze();
        session.AllPaths = FakePaths(3);
        session.CurrentPathIndex = 2;
        session.AlternativePathsComputed = true;
        session.IsAnimationMode = true;

        session.LoadImportedMaze(model);

        Assert.Multiple(() =>
        {
            Assert.That(session.AllPaths, Is.Empty);
            Assert.That(session.CurrentPathIndex, Is.Zero);
            Assert.That(session.AlternativePathsComputed, Is.False);
            Assert.That(session.IsAnimationMode, Is.False);
        });
    }

    [Test]
    public void ImportedMaze_ReportsPlaceholderStatsRatherThanNull()
    {
        // An imported maze has no generation history; the stats object must still exist so
        // consumers don't need null checks everywhere.
        var (session, model) = GenerateThenDeserialize();

        var result = session.LoadImportedMaze(model);

        Assert.Multiple(() =>
        {
            Assert.That(result.HeuristicsResults.Stats, Is.Not.Null);
            Assert.That(result.HeuristicsResults.Stats.DirectionsUsed, Is.Empty);
            Assert.That(result.DeadEndFillerResults.TotalCellsFilledIn, Is.Zero);
            Assert.That(result.AgentResults, Is.Null);
            Assert.That(result.TotalTime, Is.EqualTo(TimeSpan.Zero));
        });
    }

    #endregion

    #region Multi-step journeys

    [Test]
    public void Journey_GenerateNavigateRegenerate_KeepsStateCoherent()
    {
        // The kind of sequence only an integration test covers: each step is fine alone, but
        // the interaction is where state leaks.
        var session = NewSession(x: 6, y: 6, z: 4);

        session.GenerateMaze();
        session.SetLevel(3);
        session.AllPaths = FakePaths(2);
        session.NextPath();
        Assert.That(session.CurrentPathIndex, Is.EqualTo(1), "precondition");

        // Regenerating resets the level but does NOT itself clear paths — the caller does that
        // via ResetVisualizationState. Pinning the actual contract rather than an assumption.
        session.Settings.Seed = 99;
        session.GenerateMaze();

        Assert.Multiple(() =>
        {
            Assert.That(session.CurrentLevel, Is.Zero, "a new maze returns to level 0");
            Assert.That(session.CurrentMaze!.Seed, Is.EqualTo(99));
        });

        session.ResetVisualizationState();
        Assert.That(session.AllPaths, Is.Empty);
    }

    [Test]
    public void Journey_GenerateThenImportThenRegenerate_TracksSizeCorrectly()
    {
        var session = NewSession(x: 12, y: 12);
        session.GenerateMaze();
        Assert.That(session.Settings.Size.X, Is.EqualTo(12), "precondition");

        var (_, model) = GenerateThenDeserialize();   // an 8x8 maze
        session.LoadImportedMaze(model);
        Assert.That(session.Settings.Size.X, Is.EqualTo(8), "import adopts its own size");

        // A later regeneration must use the adopted size, not the original one.
        var regenerated = session.GenerateMaze();
        Assert.That(regenerated.HeuristicsResults.TotalCells, Is.EqualTo(64));
    }

    [Test]
    public void Journey_AnimationPlaybackDrivenThroughTheSession()
    {
        // Ties the two newly-testable pieces together: session state holding a controller that
        // is then driven by frame time, all without an engine.
        var session = NewSession();
        session.GenerateMaze();
        session.AnimationSteps = Enumerable.Range(0, 4)
            .Select(i => new AlgorithmStep { Description = $"s{i}" }).ToList();
        session.AnimationController = new AnimationController(session.AnimationSteps);
        session.IsAnimationMode = true;

        session.AnimationController.Play();
        for (var i = 0; i < 10; i++)
        {
            session.AnimationController.Update(0.5f);
        }

        Assert.Multiple(() =>
        {
            Assert.That(session.AnimationController.State, Is.EqualTo(PlaybackState.Finished));
            Assert.That(session.AnimationController.IsAtEnd, Is.True);
            Assert.That(session.IsAnimationMode, Is.True, "session flag is independent of playback");
        });
    }

    #endregion
}

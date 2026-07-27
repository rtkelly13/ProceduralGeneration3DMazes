using NUnit.Framework;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.Maze.Solver.Heuristics;

namespace ProceduralMaze.Tests;

/// <summary>
/// Guards the property that makes golden-file regression testing possible: a seed plus
/// settings fully determines the generated maze.
///
/// Before seeding existed, every algorithm produced a different maze on each run
/// (8/8 unique outputs across 8 runs) and the shortest-path length for identical settings
/// ranged from 1 to 151. Nothing downstream could assert on generated output.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class DeterminismTests
{
    private const int Seed = 20260725;

    private static readonly Algorithm[] Algorithms =
    [
        Algorithm.GrowingTreeAlgorithm,
        Algorithm.RecursiveBacktrackerAlgorithm,
        Algorithm.BinaryTreeAlgorithm,
        Algorithm.PrimsAlgorithm
    ];

    private static MazeGenerationSettings Settings(Algorithm algorithm, int? seed) => new()
    {
        Algorithm = algorithm,
        Size = new MazeSize { X = 12, Y = 12, Z = 2 },
        Option = MazeType.ArrayBidirectional,
        DoorsAtEdge = true,
        WallRemovalPercent = 0,
        AgentType = AgentType.None,
        SolverType = SolverType.Dijkstra,
        HeuristicType = HeuristicType.Manhattan,
        Seed = seed,
        GrowingTreeSettings = new GrowingTreeSettings { NewestWeight = 50, OldestWeight = 25, RandomWeight = 25 }
    };

    /// <summary>Serialised maze structure — the thing a golden file would store.</summary>
    private static string Fingerprint(ServiceContainer services, MazeGenerationResults result) =>
        services.MazeSerializer.SerializeToString(result.MazeJumper.GetModel());

    [Test]
    public void SameSeed_SameSettings_ProducesIdenticalMaze([ValueSource(nameof(Algorithms))] Algorithm algorithm)
    {
        var fingerprints = new HashSet<string>();
        for (var run = 0; run < 5; run++)
        {
            var services = new ServiceContainer();
            var result = services.MazeGenerationFactory.GenerateMaze(Settings(algorithm, Seed));
            fingerprints.Add(Fingerprint(services, result));
        }

        Assert.That(fingerprints, Has.Count.EqualTo(1),
            $"{algorithm} produced {fingerprints.Count} distinct mazes from the same seed — generation is not deterministic.");
    }

    [Test]
    public void SameSeed_ProducesIdenticalStartAndEndPoints([ValueSource(nameof(Algorithms))] Algorithm algorithm)
    {
        var endpoints = new HashSet<string>();
        for (var run = 0; run < 5; run++)
        {
            var services = new ServiceContainer();
            var r = services.MazeGenerationFactory.GenerateMaze(Settings(algorithm, Seed));
            endpoints.Add($"{r.MazeJumper.StartPoint.X},{r.MazeJumper.StartPoint.Y},{r.MazeJumper.StartPoint.Z}" +
                          $"->{r.MazeJumper.EndPoint.X},{r.MazeJumper.EndPoint.Y},{r.MazeJumper.EndPoint.Z}");
        }

        Assert.That(endpoints, Has.Count.EqualTo(1), "Start/end placement is not seed-stable.");
    }

    [Test]
    public void SameSeed_ProducesIdenticalHeuristics([ValueSource(nameof(Algorithms))] Algorithm algorithm)
    {
        var lengths = new HashSet<int>();
        for (var run = 0; run < 5; run++)
        {
            var services = new ServiceContainer();
            var r = services.MazeGenerationFactory.GenerateMaze(Settings(algorithm, Seed));
            lengths.Add(r.HeuristicsResults.ShortestPathResult.ShortestPath);
        }

        Assert.That(lengths, Has.Count.EqualTo(1),
            $"Shortest-path length varied across identical seeds: [{string.Join(", ", lengths)}]");
    }

    [Test]
    public void DifferentSeeds_ProduceDifferentMazes([ValueSource(nameof(Algorithms))] Algorithm algorithm)
    {
        // The counterpart to the tests above: seeding must not accidentally collapse
        // every run onto one maze. Distinct seeds should still explore the space.
        var fingerprints = new HashSet<string>();
        for (var seed = 1; seed <= 5; seed++)
        {
            var services = new ServiceContainer();
            var result = services.MazeGenerationFactory.GenerateMaze(Settings(algorithm, seed));
            fingerprints.Add(Fingerprint(services, result));
        }

        Assert.That(fingerprints, Has.Count.GreaterThan(1),
            $"{algorithm} produced the same maze for 5 different seeds — the seed is being ignored.");
    }

    [Test]
    public void UnseededRun_ReportsTheSeedItUsed_AndThatSeedReproducesTheMaze()
    {
        // The reproduce-after-the-fact path: a run with no seed must still report a
        // concrete seed that regenerates the identical maze. This is what makes a
        // randomly-discovered bug reportable.
        var first = new ServiceContainer();
        var original = first.MazeGenerationFactory.GenerateMaze(
            Settings(Algorithm.RecursiveBacktrackerAlgorithm, seed: null));

        Assert.That(original.Seed, Is.Not.Zero, "An unseeded run must still report the seed it used.");

        var second = new ServiceContainer();
        var reproduced = second.MazeGenerationFactory.GenerateMaze(
            Settings(Algorithm.RecursiveBacktrackerAlgorithm, seed: original.Seed));

        Assert.That(Fingerprint(second, reproduced), Is.EqualTo(Fingerprint(first, original)),
            $"Replaying reported seed {original.Seed} did not reproduce the original maze.");
    }

    [Test]
    public void SeededGeneration_IsStableAcrossWallRemovalAndAgents()
    {
        // Wall removal and agent walks are separate consumers of randomness; a seed has to
        // pin those too, or a golden file covering a full pipeline run would still flake.
        var settings = Settings(Algorithm.RecursiveBacktrackerAlgorithm, Seed);
        settings.WallRemovalPercent = 10;
        settings.AgentType = AgentType.Perfect;

        var fingerprints = new HashSet<string>();
        var agentPathLengths = new HashSet<int>();
        for (var run = 0; run < 5; run++)
        {
            var services = new ServiceContainer();
            var r = services.MazeGenerationFactory.GenerateMaze(settings);
            fingerprints.Add(Fingerprint(services, r));
            agentPathLengths.Add(r.AgentResults?.Movements.Count ?? -1);
        }

        Assert.Multiple(() =>
        {
            Assert.That(fingerprints, Has.Count.EqualTo(1), "Wall removal is not seed-stable.");
            Assert.That(agentPathLengths, Has.Count.EqualTo(1),
                $"Agent walk is not seed-stable: [{string.Join(", ", agentPathLengths)}]");
        });
    }

    [Test]
    public void SeedIsReportedBack_EvenWhenSpecified()
    {
        // Results always carry the seed that produced them, so a golden file can record
        // which seed it was generated from.
        var services = new ServiceContainer();
        var result = services.MazeGenerationFactory.GenerateMaze(
            Settings(Algorithm.GrowingTreeAlgorithm, Seed));

        Assert.That(result.Seed, Is.EqualTo(Seed));
    }

    [Test]
    public void OneContainer_ManySeeds_StaysDeterministic()
    {
        // Golden-file suites reuse a container across cases. Reseeding happens per
        // GenerateMaze call, so earlier runs must not bleed into later ones — generating
        // A then B must give the same B as generating B alone.
        var shared = new ServiceContainer();
        shared.MazeGenerationFactory.GenerateMaze(Settings(Algorithm.PrimsAlgorithm, 111));
        var bAfterA = Fingerprint(shared,
            shared.MazeGenerationFactory.GenerateMaze(Settings(Algorithm.PrimsAlgorithm, 222)));

        var fresh = new ServiceContainer();
        var bAlone = Fingerprint(fresh,
            fresh.MazeGenerationFactory.GenerateMaze(Settings(Algorithm.PrimsAlgorithm, 222)));

        Assert.That(bAfterA, Is.EqualTo(bAlone),
            "Generation order affected output — reseeding is leaking state between runs.");
    }
}

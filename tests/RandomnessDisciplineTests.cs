using System.Text.RegularExpressions;
using NUnit.Framework;

namespace ProceduralMaze.Tests;

/// <summary>
/// Architecture test: keeps maze logic free of ungoverned global randomness.
///
/// Determinism is a property of the whole pipeline — one <c>Random.Shared</c> call
/// anywhere in generation silently breaks seed reproducibility, and the symptom shows up
/// as a flaky golden-file test far from the cause. This scans the source instead of
/// relying on reviewers to notice.
///
/// If this fails: inject <c>IRandomValueGenerator</c> and use its <c>GetNext</c> /
/// <c>Shuffle</c> members rather than static randomness.
/// </summary>
[TestFixture]
public class RandomnessDisciplineTests
{
    /// <summary>
    /// Patterns that bypass the injected generator or clock.
    /// </summary>
    private static readonly (string Pattern, string Why)[] Banned =
    [
        (@"Random\s*\.\s*Shared", "Random.Shared is process-global and cannot be seeded"),
        (@"new\s+Random\s*\(", "a locally-constructed Random escapes the seeded sequence"),
        (@"ArrayHelper\s*\.\s*Shuffle", "ArrayHelper.Shuffle used Random.Shared and has been removed; use IRandomValueGenerator.Shuffle"),
        (@"Guid\s*\.\s*NewGuid", "Guid.NewGuid is nondeterministic"),
        (@"DateTime\s*\.\s*(Now|UtcNow)", "wall-clock reads make output unreproducible"),
    ];

    /// <summary>
    /// The designated sources of nondeterminism. Each is injected, so tests can pin it.
    /// Adding to this list means adding a new global-state escape hatch — think twice.
    /// </summary>
    private static readonly string[] Exempt = ["RandomValueGenerator.cs", "SystemClock.cs"];

    private static string MazeSourceRoot()
    {
        // Walk up from the test output directory to the repo root, then into scripts/maze.
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "scripts", "maze")))
        {
            dir = dir.Parent;
        }

        Assert.That(dir, Is.Not.Null, "Could not locate scripts/maze from the test directory.");
        return Path.Combine(dir!.FullName, "scripts", "maze");
    }

    [Test]
    public void MazeLogic_ContainsNoUngovernedRandomness()
    {
        var root = MazeSourceRoot();
        var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(f => !Exempt.Contains(Path.GetFileName(f)))
            .ToList();

        Assert.That(files, Is.Not.Empty, $"No source files found under {root} — the scan would vacuously pass.");

        var violations = new List<string>();
        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Skip comments and doc comments — this file and the interface docs
                // legitimately name the banned APIs when explaining why they're banned.
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("//") || trimmed.StartsWith("///") || trimmed.StartsWith("*")) continue;

                foreach (var (pattern, why) in Banned)
                {
                    if (Regex.IsMatch(line, pattern))
                    {
                        violations.Add($"{Path.GetFileName(file)}:{i + 1}: {trimmed}\n      -> {why}");
                    }
                }
            }
        }

        Assert.That(violations, Is.Empty,
            $"Ungoverned randomness in maze logic ({violations.Count} site(s)):\n  " +
            string.Join("\n  ", violations) +
            "\n\nInject IRandomValueGenerator and use GetNext/Shuffle instead.");
    }

    [Test]
    public void EveryRandomnessConsumer_ResolvesThroughTheContainer()
    {
        // Catches the other half: a class that takes IRandomValueGenerator but was wired
        // up with a throwaway instance instead of the container's, which would sit outside
        // the reseeded sequence.
        var root = MazeSourceRoot();
        var offenders = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(f => !Exempt.Contains(Path.GetFileName(f)))
            .Where(f => Regex.IsMatch(File.ReadAllText(f), @"new\s+RandomValueGenerator\s*\("))
            .Select(Path.GetFileName)
            .ToList();

        Assert.That(offenders, Is.Empty,
            "These files construct their own RandomValueGenerator instead of taking the " +
            "injected one, so they won't follow the seeded sequence: " + string.Join(", ", offenders));
    }
}

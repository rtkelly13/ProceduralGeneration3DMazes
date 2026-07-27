using NUnit.Framework;
using ProceduralMaze.Build;

namespace ProceduralMaze.Tests;

/// <summary>
/// Covers the build-identity logic behind the About screen.
/// </summary>
/// <remarks>
/// The value of this type is entirely in <em>not lying</em>: a build that cannot be traced must
/// say so, and one that can must reproduce the commit exactly. Both directions are asserted here.
/// </remarks>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class BuildInfoTests
{
    private const string RealSha = "1234567890abcdef1234567890abcdef12345678";
    private const string Repo = "rtkelly13/ProceduralGeneration3DMazes";

    private static BuildInfo Stamped(string? commit = RealSha, string? runId = "30194540175") =>
        new(commit, "main", Repo, runId, "1", "2026-07-26T09:15:03Z");

    [Test]
    public void UnstampedBuild_IsNotOfficialAndSaysSo()
    {
        var info = new BuildInfo();

        Assert.Multiple(() =>
        {
            Assert.That(info.IsOfficial, Is.False);
            Assert.That(info.ShortCommit, Is.EqualTo(BuildInfo.Unknown));
            Assert.That(info.Summary, Is.EqualTo("local development build"));
            Assert.That(info.CommitUrl, Is.Empty);
            Assert.That(info.RunUrl, Is.Empty);
        });
    }

    [Test]
    public void UnstampedBuild_DisplaysProvenanceWarning()
    {
        var rows = new BuildInfo().DisplayRows();

        Assert.That(rows, Has.Exactly(1).Matches<KeyValuePair<string, string>>(r => r.Key == "Provenance"),
            "A build that cannot be traced to a commit must say so rather than look official.");
        Assert.That(rows, Has.Exactly(1).Matches<KeyValuePair<string, string>>(
            r => r.Key == "Commit" && r.Value == BuildInfo.Unknown));
    }

    [Test]
    public void StampedBuild_IsOfficialAndExposesTheFullCommit()
    {
        var info = Stamped();

        Assert.Multiple(() =>
        {
            Assert.That(info.IsOfficial, Is.True);
            Assert.That(info.Commit, Is.EqualTo(RealSha), "The full hash must survive verbatim — it is the thing being verified.");
            Assert.That(info.ShortCommit, Is.EqualTo("1234567"));
            Assert.That(info.CommitUrl, Is.EqualTo($"https://github.com/{Repo}/commit/{RealSha}"));
            Assert.That(info.RunUrl, Is.EqualTo($"https://github.com/{Repo}/actions/runs/30194540175"));
        });
    }

    [Test]
    public void StampedBuild_OmitsTheProvenanceWarning()
    {
        Assert.That(Stamped().DisplayRows(),
            Has.None.Matches<KeyValuePair<string, string>>(r => r.Key == "Provenance"));
    }

    [Test]
    public void Summary_CombinesCommitBranchAndTime()
    {
        Assert.That(Stamped().Summary, Is.EqualTo("1234567 (main) built 2026-07-26T09:15:03Z"));
    }

    // A short hash is what a human would paste by hand; it must not be silently promoted to a
    // verifiable identity, but it should still display rather than vanish.
    [Test]
    public void ShortCommit_IsDisplayedButNotTreatedAsOfficial()
    {
        var info = Stamped(commit: "abc1234");

        Assert.Multiple(() =>
        {
            Assert.That(info.ShortCommit, Is.EqualTo("abc1234"));
            Assert.That(info.IsOfficial, Is.False);
        });
    }

    [Test]
    public void CommitWithoutRunId_IsNotOfficial()
    {
        // A local build can set a commit; only CI can set a run id. Requiring both is what
        // stops a hand-built binary presenting itself as a traceable artefact.
        Assert.That(Stamped(runId: "").IsOfficial, Is.False);
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("1234567", false, Description = "abbreviated")]
    [TestCase("1234567890abcdef1234567890abcdef1234567", false, Description = "39 chars")]
    [TestCase("1234567890abcdef1234567890abcdef123456789", false, Description = "41 chars")]
    [TestCase("1234567890abcdef1234567890abcdef1234567g", false, Description = "non-hex")]
    [TestCase("1234567890abcdef1234567890abcdef12345678", true)]
    [TestCase("1234567890ABCDEF1234567890ABCDEF12345678", true, Description = "uppercase")]
    public void IsFullCommitSha_AcceptsOnlyA40CharHexSha(string? value, bool expected)
    {
        Assert.That(BuildInfo.IsFullCommitSha(value), Is.EqualTo(expected));
    }

    // MSBuild hands over an empty string for an unset property, and a stray newline in a
    // workflow expression would silently break a hash comparison.
    [Test]
    public void Whitespace_IsTrimmedFromEveryField()
    {
        var info = new BuildInfo("  " + RealSha + "\n", " main ", " " + Repo + " ", " 42 ", " 1 ", " 2026-07-26T09:15:03Z ");

        Assert.Multiple(() =>
        {
            Assert.That(info.Commit, Is.EqualTo(RealSha));
            Assert.That(info.Branch, Is.EqualTo("main"));
            Assert.That(info.Repository, Is.EqualTo(Repo));
            Assert.That(info.RunId, Is.EqualTo("42"));
            Assert.That(info.TimestampUtc, Is.EqualTo("2026-07-26T09:15:03Z"));
            Assert.That(info.IsOfficial, Is.True, "Trimming must happen before validation, or a padded SHA looks invalid.");
        });
    }

    [Test]
    public void NullFields_DoNotThrowAndReadAsEmpty()
    {
        var info = new BuildInfo(null, null, null, null, null, null);

        Assert.Multiple(() =>
        {
            Assert.That(info.Commit, Is.Empty);
            Assert.That(info.Branch, Is.Empty);
            Assert.That(info.DisplayRows(), Is.Not.Empty);
        });
    }

    [Test]
    public void RerunAttempt_IsShownBecauseItIsADifferentArtefact()
    {
        var rows = new BuildInfo(RealSha, "main", Repo, "42", "3", "t").DisplayRows();

        Assert.That(rows, Has.Exactly(1).Matches<KeyValuePair<string, string>>(
            r => r.Key == "CI run" && r.Value == "42 (attempt 3)"));
    }

    [Test]
    public void FirstAttempt_IsNotAnnotated()
    {
        var rows = new BuildInfo(RealSha, "main", Repo, "42", "1", "t").DisplayRows();

        Assert.That(rows, Has.Exactly(1).Matches<KeyValuePair<string, string>>(
            r => r.Key == "CI run" && r.Value == "42"));
    }

    [Test]
    public void ClipboardText_ContainsTheFullCommitAndRunLink()
    {
        var text = Stamped().ToClipboardText();

        Assert.Multiple(() =>
        {
            Assert.That(text, Does.Contain(RealSha));
            Assert.That(text, Does.Contain($"https://github.com/{Repo}/actions/runs/30194540175"));
            Assert.That(text, Does.Contain("2026-07-26T09:15:03Z"));
        });
    }

    [Test]
    public void ClipboardText_OnALocalBuild_ReportsUnknownRatherThanAnEmptyField()
    {
        Assert.That(new BuildInfo().ToClipboardText(), Is.EqualTo($"commit: {BuildInfo.Unknown}\n"));
    }

    [Test]
    public void PartialStamp_OmitsRowsItCannotFill()
    {
        // Commit but no repository: a link cannot be formed, and a broken link is worse than none.
        var info = new BuildInfo(RealSha);

        Assert.Multiple(() =>
        {
            Assert.That(info.CommitUrl, Is.Empty);
            Assert.That(info.DisplayRows(), Has.None.Matches<KeyValuePair<string, string>>(r => r.Key == "Branch"));
            Assert.That(info.DisplayRows(), Has.Exactly(1).Matches<KeyValuePair<string, string>>(
                r => r.Key == "Commit" && r.Value == RealSha));
        });
    }
}

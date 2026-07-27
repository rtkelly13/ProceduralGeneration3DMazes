using System.Collections.Generic;
using System.Globalization;

namespace ProceduralMaze.Build
{
    /// <summary>
    /// Identifies the build that produced this binary, so a deployed artefact can be traced
    /// back to the exact commit it was built from.
    /// </summary>
    /// <remarks>
    /// The values come from <c>BuildStamp</c>, generated at compile time from MSBuild
    /// properties that CI supplies (see the <c>GenerateBuildStamp</c> target in
    /// <c>ProceduralGeneration3DMazes.csproj</c>).
    ///
    /// WHY COMPILE-TIME CONSTANTS RATHER THAN A DATA FILE
    ///
    /// The obvious design is a <c>build-info.json</c> read at startup. It does not survive this
    /// project's web export: <c>export_presets.cfg</c> uses <c>export_filter="all_resources"</c>
    /// with an empty <c>include_filter</c>, so a plain <c>.json</c>/<c>.txt</c> file is not
    /// packed into the PCK at all. It would work on desktop and silently report "unknown" in the
    /// browser — the one place this information is actually needed. Constants are compiled into
    /// the assembly, so there is no packaging step to get wrong, no file I/O, and nothing for the
    /// trimmer to remove.
    ///
    /// This type is deliberately Godot-free so the ordinary NUnit suite covers it; the engine and
    /// platform rows are added by the About screen, which is the part that legitimately needs
    /// Godot.
    /// </remarks>
    public sealed class BuildInfo
    {
        /// <summary>Shown wherever a field was never supplied.</summary>
        public const string Unknown = "unknown";

        /// <summary>Length of a full git SHA-1, the only form GitHub Actions supplies.</summary>
        public const int FullCommitLength = 40;

        /// <summary>Characters of the commit shown in the short form.</summary>
        public const int ShortCommitLength = 7;

        public BuildInfo(
            string? commit = null,
            string? branch = null,
            string? repository = null,
            string? runId = null,
            string? runAttempt = null,
            string? timestampUtc = null)
        {
            Commit = Normalise(commit);
            Branch = Normalise(branch);
            Repository = Normalise(repository);
            RunId = Normalise(runId);
            RunAttempt = Normalise(runAttempt);
            TimestampUtc = Normalise(timestampUtc);
        }

        /// <summary>Full commit SHA the build came from; empty for a local build.</summary>
        public string Commit { get; }

        /// <summary>Branch or tag, with any <c>refs/heads/</c> prefix already stripped by CI.</summary>
        public string Branch { get; }

        /// <summary><c>owner/repo</c>, used to build links.</summary>
        public string Repository { get; }

        /// <summary>GitHub Actions run id that produced the build.</summary>
        public string RunId { get; }

        /// <summary>Run attempt — a re-run produces the same commit but a different artefact.</summary>
        public string RunAttempt { get; }

        /// <summary>ISO-8601 UTC build time, kept verbatim as a string.</summary>
        /// <remarks>
        /// Deliberately not parsed into a <c>DateTime</c>. The web build forces
        /// <c>InvariantGlobalization</c>, and a timestamp that renders differently in the browser
        /// than in the job log is worse than useless for verification — the whole point is that
        /// the two can be compared character by character.
        /// </remarks>
        public string TimestampUtc { get; }

        /// <summary>
        /// True when this came from CI: a full-length commit SHA and a run id. A local build has
        /// neither, and must not be able to masquerade as a traceable one.
        /// </summary>
        public bool IsOfficial => IsFullCommitSha(Commit) && RunId.Length > 0;

        /// <summary>Abbreviated commit for display, or <see cref="Unknown"/> when there is none.</summary>
        public string ShortCommit =>
            Commit.Length == 0 ? Unknown
            : Commit.Length <= ShortCommitLength ? Commit
            : Commit.Substring(0, ShortCommitLength);

        /// <summary>One-line identity, e.g. <c>a1b2c3d (main) built 2026-07-26T08:15:03Z</c>.</summary>
        public string Summary
        {
            get
            {
                if (Commit.Length == 0)
                {
                    return "local development build";
                }

                var text = ShortCommit;
                if (Branch.Length > 0)
                {
                    text += " (" + Branch + ")";
                }

                if (TimestampUtc.Length > 0)
                {
                    text += " built " + TimestampUtc;
                }

                return text;
            }
        }

        /// <summary>Link to the commit, or empty when it cannot be formed.</summary>
        public string CommitUrl =>
            Repository.Length > 0 && Commit.Length > 0
                ? string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/commit/{1}", Repository, Commit)
                : string.Empty;

        /// <summary>Link to the workflow run that built this, or empty when it cannot be formed.</summary>
        public string RunUrl =>
            Repository.Length > 0 && RunId.Length > 0
                ? string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/actions/runs/{1}", Repository, RunId)
                : string.Empty;

        /// <summary>
        /// Label/value pairs for display, in order. Fields that were never supplied are omitted
        /// rather than shown as "unknown" — except the commit, whose absence is itself the point.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> DisplayRows()
        {
            var rows = new List<KeyValuePair<string, string>>
            {
                new("Commit", Commit.Length > 0 ? Commit : Unknown),
            };

            AddIfPresent(rows, "Branch", Branch);
            AddIfPresent(rows, "Built (UTC)", TimestampUtc);
            AddIfPresent(rows, "Repository", Repository);

            if (RunId.Length > 0)
            {
                var run = RunId;
                // A re-run of the same commit produces a different artefact, so attempt > 1 is
                // worth surfacing; showing "attempt 1" on every build would just be noise.
                if (RunAttempt.Length > 0 && RunAttempt != "1")
                {
                    run += " (attempt " + RunAttempt + ")";
                }

                rows.Add(new KeyValuePair<string, string>("CI run", run));
            }

            if (!IsOfficial)
            {
                rows.Add(new KeyValuePair<string, string>(
                    "Provenance",
                    "local build — not produced by CI, so it cannot be traced to a commit"));
            }

            return rows;
        }

        /// <summary>
        /// Multi-line text identifying this build, for pasting into an issue. Callers with
        /// engine-specific detail (the About screen) append their own lines.
        /// </summary>
        public string ToClipboardText()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("commit: ").Append(Commit.Length > 0 ? Commit : Unknown).Append('\n');
            AppendLineIfPresent(sb, "branch: ", Branch);
            AppendLineIfPresent(sb, "built:  ", TimestampUtc);
            AppendLineIfPresent(sb, "run:    ", RunUrl);
            return sb.ToString();
        }

        /// <summary>
        /// True when <paramref name="value"/> is a full 40-character hex SHA-1. Used to reject a
        /// truncated or placeholder value being presented as a verifiable identity.
        /// </summary>
        public static bool IsFullCommitSha(string? value)
        {
            if (value is null || value.Length != FullCommitLength)
            {
                return false;
            }

            foreach (var c in value)
            {
                var isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!isHex)
                {
                    return false;
                }
            }

            return true;
        }

        private static void AppendLineIfPresent(System.Text.StringBuilder sb, string prefix, string value)
        {
            if (value.Length > 0)
            {
                sb.Append(prefix).Append(value).Append('\n');
            }
        }

        private static void AddIfPresent(List<KeyValuePair<string, string>> rows, string label, string value)
        {
            if (value.Length > 0)
            {
                rows.Add(new KeyValuePair<string, string>(label, value));
            }
        }

        /// <summary>
        /// MSBuild substitutes an empty string for an unset property, and a stray newline or
        /// space would corrupt a hash comparison, so every field is trimmed on the way in.
        /// </summary>
        private static string Normalise(string? value) => value is null ? string.Empty : value.Trim();
    }
}

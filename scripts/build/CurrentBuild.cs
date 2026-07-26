namespace ProceduralMaze.Build
{
    /// <summary>
    /// The build stamp of the running binary, resolved once.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="BuildInfo"/> so that type stays free of the generated
    /// <c>BuildStamp</c> constants and can be compiled — and therefore tested — by the NUnit
    /// suite, which does not run the code generator. Anything needing "which build am I?"
    /// should come here rather than touching <c>BuildStamp</c> directly.
    /// </remarks>
    public static class CurrentBuild
    {
        /// <summary>Identity of this build. Never null; reports a local build when unstamped.</summary>
        public static BuildInfo Info { get; } = new BuildInfo(
            BuildStamp.Commit,
            BuildStamp.Branch,
            BuildStamp.Repository,
            BuildStamp.RunId,
            BuildStamp.RunAttempt,
            BuildStamp.TimestampUtc);
    }
}

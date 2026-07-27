using System;

namespace ProceduralMaze.Maze.Helper
{
    /// <summary>
    /// Injected source of wall-clock time.
    /// </summary>
    /// <remarks>
    /// Exists for the same reason as <see cref="IRandomValueGenerator"/>: a direct
    /// <c>DateTime.Now</c> read inside serialization makes the output bytes differ on every
    /// run, which defeats byte-comparison against a golden file. Regression tests inject a
    /// fixed clock; production uses <see cref="SystemClock"/>.
    /// </remarks>
    public interface ISystemClock
    {
        DateTime Now { get; }
    }
}

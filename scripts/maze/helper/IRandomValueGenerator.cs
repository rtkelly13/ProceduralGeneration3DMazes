using System.Collections.Generic;

namespace ProceduralMaze.Maze.Helper
{
    /// <summary>
    /// The single source of randomness for maze generation.
    /// </summary>
    /// <remarks>
    /// Every random decision in generation MUST go through an injected instance of this
    /// interface. Reaching for <c>Random.Shared</c>, <c>new Random()</c> or the static
    /// <c>ArrayHelper.Shuffle</c> overloads inside generation code reintroduces global
    /// state that cannot be seeded, which makes runs unreproducible and golden-file
    /// regression tests impossible. <c>RandomnessDisciplineTests</c> enforces this.
    ///
    /// Instances are NOT thread-safe, deliberately: the maze pipeline is single-threaded,
    /// and a per-instance generator is what makes a seeded run reproducible. Give each
    /// concurrent pipeline its own <c>ServiceContainer</c> (as the test suite does)
    /// rather than sharing one generator across threads.
    /// </remarks>
    public interface IRandomValueGenerator
    {
        /// <summary>The seed currently driving this generator.</summary>
        int Seed { get; }

        /// <summary>Random integer in the INCLUSIVE range [min, max].</summary>
        int GetNext(int min, int max);

        /// <summary>Shuffles in place using this generator's sequence.</summary>
        void Shuffle<T>(T[] array);

        /// <summary>Shuffles in place using this generator's sequence.</summary>
        void Shuffle<T>(IList<T> list);

        /// <summary>
        /// Restarts the sequence from <paramref name="seed"/>. Called once per generation
        /// run so that the same seed always yields the same maze.
        /// </summary>
        void Reseed(int seed);
    }
}

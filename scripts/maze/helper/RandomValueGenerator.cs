using System;
using System.Collections.Generic;

namespace ProceduralMaze.Maze.Helper
{
    /// <summary>
    /// Seedable, per-instance source of randomness. See <see cref="IRandomValueGenerator"/>
    /// for the discipline this exists to enforce.
    /// </summary>
    /// <remarks>
    /// Previously this delegated to a <c>[ThreadStatic]</c> <see cref="Random"/> seeded from
    /// <c>Environment.TickCount</c>, which made every run unreproducible. The thread-static
    /// indirection also wasn't buying anything — nothing in the maze pipeline is concurrent —
    /// so a plain instance field is both simpler and seedable.
    /// </remarks>
    public class RandomValueGenerator : IRandomValueGenerator
    {
        private Random _random;

        public int Seed { get; private set; }

        /// <param name="seed">
        /// Fixed seed for reproducible output. When null, a seed is drawn from the system
        /// clock and exposed via <see cref="Seed"/> — so even an unseeded run can be
        /// reproduced after the fact.
        /// </param>
        public RandomValueGenerator(int? seed = null)
        {
            Seed = seed ?? NewRandomSeed();
            _random = new Random(Seed);
        }

        public void Reseed(int seed)
        {
            Seed = seed;
            _random = new Random(seed);
        }

        // +1 because Random.Next's max is exclusive while this contract is inclusive.
        public int GetNext(int min, int max) => _random.Next(min, max + 1);

        public void Shuffle<T>(T[] array) => _random.Shuffle(array);

        public void Shuffle<T>(IList<T> list)
        {
            if (list is List<T> concrete)
            {
                // Span path keeps the common case allocation-free, matching the old
                // ArrayHelper.Shuffle(List<T>) behaviour.
                _random.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(concrete));
                return;
            }

            // Fisher-Yates for any other IList<T>.
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Draws a fresh seed for an unseeded run. Uses a throwaway <see cref="Random"/>
        /// rather than <c>Random.Shared</c> so nothing here depends on shared global state.
        /// </summary>
        public static int NewRandomSeed() => new Random().Next();
    }
}

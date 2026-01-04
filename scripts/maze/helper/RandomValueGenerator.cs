using System;
using System.Threading;

namespace ProceduralMaze.Maze.Helper
{
    public class RandomValueGenerator : IRandomValueGenerator
    {
        public int GetNext(int min, int max)
        {
            return ThreadSafeRandom.ThisThreadsRandom.Next(min, max + 1); // +1 because Random.Next max is exclusive
        }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random? Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ??= new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)); }
        }
    }
}

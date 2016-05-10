using System;
using System.Threading;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public class RandomValueGenerator : IRandomValueGenerator
    {

        public int GetNext(int min, int max)
        {
            return ThreadSafeRandom.ThisThreadsRandom.Next(min, max);
        }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralMaze.Maze.Helper
{
    public static class ArrayHelper
    {
        public static void Shuffle<T>(T[] array)
        {
            Random.Shared.Shuffle(array);
        }

        public static void Shuffle<T>(List<T> list)
        {
            Random.Shared.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list));
        }

        public static double Average<T>(IEnumerable<T> items, Func<T, double> func)
        {
            int count = 0;
            var total = items.Aggregate(0.0, (seed, item) =>
            {
                count++;
                return seed + func(item);
            });
            return total / count;
        }
    }
}

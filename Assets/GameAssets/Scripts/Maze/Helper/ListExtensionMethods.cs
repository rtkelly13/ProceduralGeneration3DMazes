using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public static class ListExtensionMethods
    {
        public static IEnumerable<T> Randomise<T>(this IEnumerable<T> source)
        {
            var rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }
    }
}

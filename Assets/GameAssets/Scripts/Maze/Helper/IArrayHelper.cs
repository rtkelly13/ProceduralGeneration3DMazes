using System;
using System.Collections.Generic;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public interface IArrayHelper
    {
        double Average<T>(IEnumerable<T> items, Func<T, double> func);
        void Shuffle<T>(List<T> array);
    }
}
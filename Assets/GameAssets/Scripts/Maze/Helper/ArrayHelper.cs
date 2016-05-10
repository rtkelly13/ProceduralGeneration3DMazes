using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public class ArrayHelper : IArrayHelper
    {
        private readonly IRandomValueGenerator _randomValueGenerator;

        public ArrayHelper(IRandomValueGenerator randomValueGenerator)
        {
            _randomValueGenerator = randomValueGenerator;
        }

        public void Shuffle<T>(List<T> array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                int j = _randomValueGenerator.GetNext(i, array.Count);
                T tmp = array[j];
                array[j] = array[i];
                array[i] = tmp;
            }
        }

        public double Average<T>(IEnumerable<T> items, Func<T, double> func)
        {
            int count = 0;
            var total = items.Aggregate(0.0, (seed, item) =>
            {
                count++;
                return seed + func(item);
            });
            return total/count;
        }
    }
}

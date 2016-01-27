using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGeneration.Helper
{
    public class RandomValueGenerator : IRandomValueGenerator
    {
        private readonly Random _random;

        public RandomValueGenerator()
        {
            _random = new Random();
        }

        public int GetNext(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}

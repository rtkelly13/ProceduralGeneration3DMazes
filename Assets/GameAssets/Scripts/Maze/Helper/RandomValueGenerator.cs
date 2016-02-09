using System;

namespace Assets.GameAssets.Scripts.Maze.Helper
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

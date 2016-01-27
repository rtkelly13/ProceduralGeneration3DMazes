using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MazeGeneration.Factory;
using MazeGeneration.Helper;
using MazeGeneration.Model;

namespace MazeGeneration.MazeGeneration
{
    public class RandomPointGenerator : IRandomPointGenerator
    {
        private readonly IRandomValueGenerator _randomValueGenerator;
        private readonly IMazePointFactory _mazePointFactory;

        public RandomPointGenerator(IRandomValueGenerator randomValueGenerator, IMazePointFactory mazePointFactory)
        {
            _randomValueGenerator = randomValueGenerator;
            _mazePointFactory = mazePointFactory;
        }

        public MazePoint RandomPoint(MazeSize size)
        {
            var x = _randomValueGenerator.GetNext(0, size.Width - 1);
            var y = _randomValueGenerator.GetNext(0, size.Height - 1);
            var z = _randomValueGenerator.GetNext(0, size.Depth - 1);
            return _mazePointFactory.MakePoint(x,y,z);
        }
    }
}

using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
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
            var x = _randomValueGenerator.GetNext(0, size.X - 1);
            var y = _randomValueGenerator.GetNext(0, size.Y - 1);
            var z = _randomValueGenerator.GetNext(0, size.Z - 1);
            return _mazePointFactory.MakePoint(x,y,z);
        }
    }
}

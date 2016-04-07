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

        public MazePoint RandomPoint(MazeSize size, PickType type)
        {
            var x = _randomValueGenerator.GetNext(0, size.X - 1);
            var y = _randomValueGenerator.GetNext(0, size.Y - 1);
            var z = _randomValueGenerator.GetNext(0, size.Z - 1);
            if (type == PickType.RandomEdge)
            {
                switch ((RandomEdgeOptions)_randomValueGenerator.GetNext(1, 6))
                {
                    case RandomEdgeOptions.MinX:
                        x = 0;
                        break;
                    case RandomEdgeOptions.MaxX:
                        x = size.X - 1;
                        break;
                    case RandomEdgeOptions.MinY:
                        y = 0;
                        break;
                    case RandomEdgeOptions.MaxY:
                        y = size.Y - 1;
                        break;
                    case RandomEdgeOptions.MinZ:
                        z = 0;
                        break;
                    case RandomEdgeOptions.MaxZ:
                        z = size.Z - 1;
                        break;
                }
            }
            return _mazePointFactory.MakePoint(x, y, z);
        }

    }

    public enum RandomEdgeOptions
    {
        MinX = 1,
        MaxX = 2,
        MinY = 3,
        MaxY = 4,
        MinZ = 5,
        MaxZ = 6
    }
}

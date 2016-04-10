using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class MazeModelFactory : IMazeModelFactory
    {
        private readonly IMovementHelper _movementHelper;
        private readonly IDirectionsFlagParser _parser;
        private readonly IMazePointFactory _pointFactory;
        private readonly IMazeArrayBuilder _mazeArrayBuilder;
        private readonly IRandomPointGenerator _randomPointGenerator;

        public MazeModelFactory(IMovementHelper movementHelper, IDirectionsFlagParser parser, IMazePointFactory pointFactory, IMazeArrayBuilder mazeArrayBuilder, IRandomPointGenerator randomPointGenerator)
        {
            _movementHelper = movementHelper;
            _parser = parser;
            _pointFactory = pointFactory;
            _mazeArrayBuilder = mazeArrayBuilder;
            _randomPointGenerator = randomPointGenerator;
        }

        public IModelBuilder BuildMaze(MazeGenerationSettings settings)
        {
            var pickType = PickType.Random;
            if (settings.DoorsAtEdge)
            {
                pickType = PickType.RandomEdge;
            }
            var startPoint = _randomPointGenerator.RandomPoint(settings.Size, pickType);
            var endPoint = _randomPointGenerator.RandomPoint(settings.Size, pickType);
            while (startPoint.Equals(endPoint))
            {
                endPoint = _randomPointGenerator.RandomPoint(settings.Size, pickType);
            }
            var options = new ModelInitialisationOptions
            {
                Size = settings.Size,
                StartPoint = startPoint,
                EndPoint = endPoint
            };
            switch (settings.Option)
            {
                case MazeType.None:
                    throw new ArgumentException("Maze Type None is not supported");
                case MazeType.UndirectedMaze:
                    return new Model2(_parser, _movementHelper, _mazeArrayBuilder).BaseInitialise(options);
                case MazeType.DirectedMaze:
                    return new Model1(_parser, _movementHelper, _mazeArrayBuilder).BaseInitialise(options);
                case MazeType.DictionaryMaze:
                    return new Model3(_parser, _pointFactory, _movementHelper).BaseInitialise(options);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

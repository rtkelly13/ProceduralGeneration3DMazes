using System;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
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
                case MazeType.ArrayUnidirectional:
                    return new ArrayUnidirectionalModel(_parser, _movementHelper, _mazeArrayBuilder).BaseInitialise(options);
                case MazeType.ArrayBidirectional:
                    return new ArrayBidirectionalModel(_parser, _movementHelper, _mazeArrayBuilder).BaseInitialise(options);
                case MazeType.Dictionary:
                    return new DictionaryModel(_parser, _pointFactory, _movementHelper).BaseInitialise(options);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

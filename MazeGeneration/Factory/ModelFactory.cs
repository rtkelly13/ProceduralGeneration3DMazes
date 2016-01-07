using System;
using MazeGeneration.Helper;
using MazeGeneration.Model;

namespace MazeGeneration.Factory
{
    public class ModelFactory : IModelFactory
    {
        private readonly IDirectionsFlagParser _flagParser;
        private readonly IMazePointFactory _mazePointFactory;
        private readonly IMovementHelper _movementHelper;

        public ModelFactory(IDirectionsFlagParser flagParser, IMazePointFactory mazePointFactory, IMovementHelper movementHelper)
        {
            _flagParser = flagParser;
            _mazePointFactory = mazePointFactory;
            _movementHelper = movementHelper;
        }

        public IBuilder MakeModel(MazeType type, MazeSize size)
        {
            IBuilder model;
            switch (type)
            {
                case MazeType.DirectedMaze:
                    model = new MazeModel1(_flagParser, _mazePointFactory, _movementHelper);
                    break;
                case MazeType.UndirectedMaze:
                    model = new MazeModel2(_flagParser, _mazePointFactory, _movementHelper);
                    break;
                case MazeType.DictionaryMaze:
                    model = new MazeModel3(_flagParser, _mazePointFactory, _movementHelper);
                    break;
                default:
                    throw new ArgumentException("Maze Type not supported");
            }
            model.BaseInitialise(size, false);
            return model;
        }
    }
}

using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public class DeadEndModelWrapperFactory : IDeadEndModelWrapperFactory
    {
        private readonly IMazeArrayBuilder _mazeArrayBuilder;
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IMovementHelper _movementHelper;

        public DeadEndModelWrapperFactory(IMazeArrayBuilder mazeArrayBuilder, IDirectionsFlagParser directionsFlagParser, IMovementHelper movementHelper)
        {
            _mazeArrayBuilder = mazeArrayBuilder;
            _directionsFlagParser = directionsFlagParser;
            _movementHelper = movementHelper;
        }

        public IDeadEndModelWrapper MakeModel(IModelBuilder model)
        {
            return new DeadEndModelWrapper(_movementHelper, _directionsFlagParser, _mazeArrayBuilder, model);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
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

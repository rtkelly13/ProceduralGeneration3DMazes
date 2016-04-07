using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class MazeFactory : IMazeFactory
    {
        private readonly IPointValidity _pointValidity;
        private readonly IMovementHelper _movementHelper;
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IModelsWrapperFactory _modelsWrapperFactory;

        public MazeFactory(IPointValidity pointValidity, IMovementHelper movementHelper, IDirectionsFlagParser directionsFlagParser, IRandomPointGenerator randomPointGenerator, IModelsWrapperFactory modelsWrapperFactory)
        {
            _pointValidity = pointValidity;
            _movementHelper = movementHelper;
            _directionsFlagParser = directionsFlagParser;
            _randomPointGenerator = randomPointGenerator;
            _modelsWrapperFactory = modelsWrapperFactory;
        }

        public IMazeCarver GetMazeCarver(IModelBuilder modelBuilder)
        {
            var carver = new MazeCarver();
            carver.Initialise(_modelsWrapperFactory.Make(modelBuilder), _directionsFlagParser, _movementHelper, _pointValidity, _randomPointGenerator);
            return carver;
        }
    }
}

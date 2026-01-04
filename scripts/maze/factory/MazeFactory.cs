using System.Linq;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public class MazeFactory : IMazeFactory
    {
        private readonly IPointValidity _pointValidity;
        private readonly IMovementHelper _movementHelper;
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IModelsWrapperFactory _modelsWrapperFactory;
        private readonly IDeadEndModelWrapperFactory _deadEndModelWrapperFactory;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;

        public MazeFactory(
            IPointValidity pointValidity,
            IMovementHelper movementHelper,
            IDirectionsFlagParser directionsFlagParser,
            IRandomPointGenerator randomPointGenerator,
            IModelsWrapperFactory modelsWrapperFactory,
            IDeadEndModelWrapperFactory deadEndModelWrapperFactory,
            IPointsAndDirectionsRetriever pointsAndDirectionsRetriever)
        {
            _pointValidity = pointValidity;
            _movementHelper = movementHelper;
            _directionsFlagParser = directionsFlagParser;
            _randomPointGenerator = randomPointGenerator;
            _modelsWrapperFactory = modelsWrapperFactory;
            _deadEndModelWrapperFactory = deadEndModelWrapperFactory;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
        }

        public IMazeCarver GetMazeCarver(IModelBuilder modelBuilder)
        {
            var carver = new MazeCarver();
            carver.Initialise(_modelsWrapperFactory.Make(modelBuilder), _directionsFlagParser, _movementHelper, _pointValidity, _randomPointGenerator);
            return carver;
        }

        public IMazeJumper GetMazeJumperFromModel(IModel model, bool fillDeadEnds = true)
        {
            var wrapper = new ReadOnlyModelsWrapper(model);
            // Initialize dead-end wrapping so the jumper can be used with solvers
            wrapper.DoDeadEndWrapping(mb => _deadEndModelWrapperFactory.MakeModel(mb));
            
            var jumper = new MazeJumper();
            jumper.Initialise(wrapper, _directionsFlagParser, _movementHelper, _pointValidity, _randomPointGenerator, model.StartPoint);
            
            if (fillDeadEnds)
            {
                FillDeadEndsForJumper(jumper, wrapper);
            }
            
            return jumper;
        }

        /// <summary>
        /// Fills dead ends for a jumper by directly manipulating the DeadEndModelWrapper.
        /// This allows imported mazes to support dead-end hiding without needing carve capabilities.
        /// </summary>
        private void FillDeadEndsForJumper(IMazeJumper jumper, ReadOnlyModelsWrapper wrapper)
        {
            wrapper.SetState(ModelMode.DeadEndFilled);
            var deadEndsRemaining = true;
            
            while (deadEndsRemaining)
            {
                var pointsAndDirections = _pointsAndDirectionsRetriever
                    .GetDeadEnds(jumper)
                    .Where(x => !_pointsAndDirectionsRetriever.PointIsStartOrEnd(x.Point, jumper.StartPoint, jumper.EndPoint))
                    .ToList();
                    
                if (!pointsAndDirections.Any())
                {
                    deadEndsRemaining = false;
                }
                else
                {
                    foreach (var pointAndDirection in pointsAndDirections)
                    {
                        jumper.JumpToPoint(pointAndDirection.Point);
                        FillPassageFromPoint(jumper, wrapper);
                    }
                }
            }
            
            // Reset to start point and standard mode
            jumper.JumpToPoint(jumper.StartPoint);
            wrapper.SetState(ModelMode.Standard);
        }

        /// <summary>
        /// Fills in a passage starting from a dead-end point until reaching a junction or start/end.
        /// </summary>
        private void FillPassageFromPoint(IMazeJumper jumper, ReadOnlyModelsWrapper wrapper)
        {
            var junctionOrStartOrEndReached = false;
            
            do
            {
                var directions = jumper.GetDirectionsFromPoint();
                if (directions.Length == 1)
                {
                    var direction = directions[0];
                    // Remove the vertex in the dead-end model wrapper
                    wrapper.ModelBuilder.RemoveVertex(jumper.CurrentPoint, direction);
                    jumper.JumpInDirection(direction);
                    
                    if (_pointsAndDirectionsRetriever.PointIsStartOrEnd(jumper.CurrentPoint, jumper.StartPoint, jumper.EndPoint))
                    {
                        junctionOrStartOrEndReached = true;
                    }
                }
                else
                {
                    junctionOrStartOrEndReached = true;
                }
            } while (!junctionOrStartOrEndReached);
        }
    }
}

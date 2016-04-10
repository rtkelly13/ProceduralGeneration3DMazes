using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class Maze : IMaze
    {
        protected IModelsWrapper ModelsWrapper;
        protected IDirectionsFlagParser DirectionsFlagParser;
        protected IMovementHelper MovementHelper;
        protected IPointValidity PointValidity;
        protected IRandomPointGenerator RandomPointGenerator;

        public MazePoint CurrentPoint { get; protected set; }

        public MazeSize Size
        {
            get { return ModelsWrapper.ModelBuilder.Size; }
        }

        public MazePoint StartPoint
        {
            get { return ModelsWrapper.ModelBuilder.StartPoint; }
        }

        public MazePoint EndPoint
        {
            get { return ModelsWrapper.ModelBuilder.EndPoint; }
        }

        public bool DeadEnded
        {
            get { return ModelsWrapper.DeadEnded; }
        }

        public ModelMode ModelMode
        {
            get { return ModelsWrapper.ModelMode; }
        }

        public void SetState(ModelMode mode)
        {
            ModelsWrapper.SetState(mode);
        }

        public void DoDeadEndWrapping(Func<IModelBuilder, IDeadEndModelWrapper> modelAction)
        {
            ModelsWrapper.DoDeadEndWrapping(modelAction);
        }

        public Direction GetFlagFromPoint()
        {
            return ModelsWrapper.ModelBuilder.GetFlagFromPoint(CurrentPoint);
        }

        public IEnumerable<Direction> GetsDirectionsFromPoint()
        {
            return DirectionsFlagParser.SplitDirectionsFromFlag(GetFlagFromPoint());
        }

        public bool HasVertexes(Direction flag)
        {
            return DirectionsFlagParser.FlagHasDirections(GetFlagFromPoint(), flag);
        }

        public void MoveInDirection(Direction d)
        {
            if (CanMoveInDirection(d))
            {
                CurrentPoint = MovementHelper.Move(CurrentPoint, d, Size);
            }
            else
            {
                throw new ArgumentException("There is no passage cannot move in that direction");
            }
        }

        public bool CanMoveInDirection(Direction d)
        {
            return ModelsWrapper.ModelBuilder.HasDirections(CurrentPoint, d);
        }

        public void Initialise(IModelsWrapper modelsWrapper, IDirectionsFlagParser directionsFlagParser, IMovementHelper movementHelper, IPointValidity pointValidity, IRandomPointGenerator randomPointGenerator, MazePoint startingPoint = null)
        {
            ModelsWrapper = modelsWrapper;
            DirectionsFlagParser = directionsFlagParser;
            MovementHelper = movementHelper;
            PointValidity = pointValidity;
            RandomPointGenerator = randomPointGenerator;
            CurrentPoint = startingPoint ?? randomPointGenerator.RandomPoint(Size);
        }
    }
}

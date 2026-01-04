using System;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Helper;

namespace ProceduralMaze.Maze.Model
{
    public class Maze : IMaze
    {
        protected IModelsWrapper ModelsWrapper = null!;
        protected IDirectionsFlagParser DirectionsFlagParser = null!;
        protected IMovementHelper MovementHelper = null!;
        protected IPointValidity PointValidity = null!;
        protected IRandomPointGenerator RandomPointGenerator = null!;

        public MazePoint CurrentPoint { get; protected set; } = null!;

        public MazeSize Size => ModelsWrapper.ModelBuilder.Size;

        public MazePoint StartPoint => ModelsWrapper.ModelBuilder.StartPoint;

        public MazePoint EndPoint => ModelsWrapper.ModelBuilder.EndPoint;

        public bool DeadEnded => ModelsWrapper.DeadEnded;

        public ModelMode ModelMode => ModelsWrapper.ModelMode;

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

        public Direction[] GetDirectionsFromPoint()
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

        public void Initialise(IModelsWrapper modelsWrapper, IDirectionsFlagParser directionsFlagParser, 
            IMovementHelper movementHelper, IPointValidity pointValidity, 
            IRandomPointGenerator randomPointGenerator, MazePoint? startingPoint = null)
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

namespace ProceduralMaze.Maze.Model
{
    public class MazeCarver : MazeJumper, IMazeCarver
    {
        public Direction[] CarvableDirections()
        {
            return DirectionsFlagParser.SplitDirectionsFromFlag(CarvableFlag());
        }

        public Direction CarvableFlag()
        {
            var jumpableFlag = JumpableFlag();
            var flag = GetFlagFromPoint();
            var carvableFlag = DirectionsFlagParser.FlagUnion(jumpableFlag, DirectionsFlagParser.OppositeFlag(flag));
            return carvableFlag;
        }

        public Direction[] AlreadyCarvedDirections()
        {
            return GetDirectionsFromPoint();
        }

        public Direction AlreadyCarvedFlag()
        {
            return GetFlagFromPoint();
        }

        public bool CanCarveInDirection(Direction d)
        {
            return DirectionsFlagParser.FlagHasDirections(CarvableFlag(), d);
        }

        public void CarveInDirection(Direction d)
        {
            if (CanCarveInDirection(d))
            {
                ModelsWrapper.ModelBuilder.PlaceVertex(CurrentPoint, d);
            }
        }

        public void FillInDirection(Direction d)
        {
            if (CanMoveInDirection(d))
            {
                ModelsWrapper.ModelBuilder.RemoveVertex(CurrentPoint, d);
            }
        }

        public bool AlreadyCarvedDirection(Direction d)
        {
            return DirectionsFlagParser.FlagHasDirections(GetFlagFromPoint(), d);
        }

        public IMazeJumper CarvingFinished()
        {
            var jumper = new MazeJumper();
            jumper.Initialise(ModelsWrapper, DirectionsFlagParser, MovementHelper, PointValidity, RandomPointGenerator, CurrentPoint);
            return jumper;
        }
    }
}

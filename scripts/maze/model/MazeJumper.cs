using System;

namespace ProceduralMaze.Maze.Model
{
    public class MazeJumper : Maze, IMazeJumper
    {
        public Direction[] JumpableDirections()
        {
            return MovementHelper.AdjacentPoints(CurrentPoint, Size);
        }

        public Direction JumpableFlag()
        {
            return MovementHelper.AdjacentPointsFlag(CurrentPoint, Size);
        }

        public bool CanJumpInDirection(Direction d)
        {
            var directions = JumpableDirections();
            foreach (var dir in directions)
            {
                if (d == dir) return true;
            }
            return false;
        }

        public bool TryJumpInDirection(Direction d)
        {
            if (CanJumpInDirection(d))
            {
                CurrentPoint = MovementHelper.Move(CurrentPoint, d, Size);
                return true;
            }
            return false;
        }

        public void JumpInDirection(Direction d)
        {
            if (CanJumpInDirection(d))
            {
                CurrentPoint = MovementHelper.Move(CurrentPoint, d, Size);
            }
            else
            {
                throw new ArgumentException("Cannot Jump in that direction.");
            }
        }

        public bool CanJumpToPoint(MazePoint p)
        {
            return PointValidity.ValidPoint(p, Size);
        }

        public bool TryJumpToPoint(MazePoint p)
        {
            if (CanJumpToPoint(p))
            {
                JumpToPoint(p);
                return true;
            }
            return false;
        }

        public void JumpToPoint(MazePoint p)
        {
            CurrentPoint = MovementHelper.Move(CurrentPoint, p, Size);
        }

        public IMaze JumpingFinished()
        {
            var maze = new Maze();
            maze.Initialise(ModelsWrapper, DirectionsFlagParser, MovementHelper, PointValidity, RandomPointGenerator, CurrentPoint);
            return maze;
        }
        
        public IModel GetModel()
        {
            return ModelsWrapper.ModelBuilder;
        }
    }
}

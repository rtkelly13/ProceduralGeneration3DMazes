using System;
using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class MazeJumper: Maze, IMazeJumper
    {
        public MazeJumper(MazeSize size, MazePoint startingPoint) : base(size, startingPoint)
        {
        }

        public IEnumerable<Direction> JumpableDirections()
        {
            throw new NotImplementedException();
        }

        public Direction JumpableFlag()
        {
            throw new NotImplementedException();
        }

        public bool CanJumpInDirection(Direction d)
        {
            throw new NotImplementedException();
        }

        public bool TryJumpInDirection(Direction d)
        {
            throw new NotImplementedException();
        }

        public void JumpInDirection(Direction d)
        {
            throw new NotImplementedException();
        }

        public bool CanJumpToPoint(MazePoint p)
        {
            throw new NotImplementedException();
        }

        public bool TryJumpToPoint(MazePoint p)
        {
            throw new NotImplementedException();
        }

        public void JumpToPoint(MazePoint p)
        {
            throw new NotImplementedException();
        }

        public IMaze JumpingFinished()
        {
            throw new NotImplementedException();
        }
    }
}
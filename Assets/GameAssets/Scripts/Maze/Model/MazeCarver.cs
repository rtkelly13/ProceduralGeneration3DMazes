using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class MazeCarver: MazeJumper, IMazeCarver
    {
        public MazeCarver(MazeSize size, MazePoint startingPoint) : base(size, startingPoint)
        {
        }

        public IEnumerable<Direction> CarvableDirections()
        {
            throw new NotImplementedException();
        }

        public Direction CarvableFlag()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Direction> AlreadyCarvedDirections()
        {
            throw new NotImplementedException();
        }

        public Direction AlreadyCarvedFlag()
        {
            throw new NotImplementedException();
        }

        public bool CanCarveInDirection(Direction d)
        {
            throw new NotImplementedException();
        }

        public void CarveInDirection(Direction d)
        {
            throw new NotImplementedException();
        }

        public bool AlreadyCarvedDirection(Direction d)
        {
            throw new NotImplementedException();
        }

        public IMazeJumper CarvingFinished()
        {
            throw new NotImplementedException();
        }
    }
}

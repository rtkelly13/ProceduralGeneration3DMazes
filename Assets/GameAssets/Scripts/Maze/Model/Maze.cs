using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class Maze : IMaze
    {
        public Maze(MazeSize size, MazePoint startingPoint)
        {
            Size = size;
            CurrentPoint = startingPoint;
        }
        public Direction GetFlagFromPoint()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Direction> GetsDirectionsFromPoint()
        {
            throw new NotImplementedException();
        }

        public bool HasVertexes(Direction flag)
        {
            throw new NotImplementedException();
        }

        public void MoveInDirection(Direction d)
        {
            throw new NotImplementedException();
        }

        public MazePoint CurrentPoint { get; protected set; }
        public MazeSize Size { get; protected set; }
    }
}

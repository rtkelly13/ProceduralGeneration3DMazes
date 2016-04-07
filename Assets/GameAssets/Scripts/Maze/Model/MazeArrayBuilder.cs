using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class MazeArrayBuilder : IMazeArrayBuilder
    {
        private readonly IMazePointFactory _pointFactory;

        public MazeArrayBuilder(IMazePointFactory pointFactory)
        {
            _pointFactory = pointFactory;
        }

        public MazeCell[,,] Build(MazeSize size)
        {
            var maze = new MazeCell[size.X, size.Y, size.Z];
            var xValues = Enumerable.Range(0, size.X).ToList();
            var yValues = Enumerable.Range(0, size.Y).ToList();
            var zValues = Enumerable.Range(0, size.Z).ToList();
            foreach (var point in xValues.SelectMany(x => yValues.SelectMany(y => zValues.Select(z => _pointFactory.MakePoint(x, y, z)))))
            {
                maze[point.X, point.Y, point.Z] = new MazeCell
                {
                    Directions = Direction.None
                };
            }
            return maze;
        }
    }
}

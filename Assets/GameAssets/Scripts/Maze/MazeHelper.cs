using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public class MazeHelper : IMazeHelper
    {
        private readonly IMazePointFactory _pointFactory;

        public MazeHelper(IMazePointFactory pointFactory)
        {
            _pointFactory = pointFactory;
        }

        public IEnumerable<T> GetForEachPoint<T>(MazeSize size, Func<MazePoint, T> function)
        {
            var xValues = Enumerable.Range(0, size.X).ToList();
            var yValues = Enumerable.Range(0, size.Y).ToList();
            var zValues = Enumerable.Range(0, size.Z).ToList();
            return xValues.SelectMany(x => yValues.SelectMany(y => zValues.Select(z => _pointFactory.MakePoint(x, y, z)))).Select(point => function(point));
        }

        public IEnumerable<T> GetForEachZ<T>(MazeSize size, int z, Func<MazePoint, T> function)
        {
            var xValues = Enumerable.Range(0, size.X).ToList();
            var yValues = Enumerable.Range(0, size.Y).ToList();
            return xValues.SelectMany(x => yValues.Select(y  => _pointFactory.MakePoint(x, y, z))).Select(point => function(point));
        }

        public IEnumerable<MazePoint> GetPoints(MazeSize size, Func<MazePoint, bool> function)
        {
            var points = GetForEachPoint(size, x => x);
            return points.Where(function);
        }

        public void DoForEachPoint(MazeSize size, Action<MazePoint> action)
        {
            var points = GetForEachPoint(size, x => x);
            foreach (var point in points)
            {
                action(point);
            }
        }

        public void DoForEachZ(MazeSize size, int z, Action<MazePoint> action)
        {
            var points = GetForEachZ(size, z, x => x);
            foreach (var point in points)
            {
                action(point);
            }
        }
    }
}

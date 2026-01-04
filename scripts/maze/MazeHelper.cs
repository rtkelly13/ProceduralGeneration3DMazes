using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze
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
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        yield return function(_pointFactory.MakePoint(x, y, z));
                    }
                }
            }
        }

        public IEnumerable<MazePoint> GetForEachPoint<T>(MazeSize size, Func<MazePoint, bool> func)
        {
            return GetForEachPoint(size, x => x).Where(func);
        }

        public IEnumerable<T> GetForEachZ<T>(MazeSize size, int z, Func<MazePoint, T> function)
        {
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    yield return function(_pointFactory.MakePoint(x, y, z));
                }
            }
        }

        public IEnumerable<MazePoint> GetForEachZ<T>(MazeSize size, int z, Func<MazePoint, bool> func)
        {
            return GetForEachZ(size, z, x => x).Where(func);
        }

        public IEnumerable<MazePoint> GetPoints(MazeSize size, Func<MazePoint, bool> function)
        {
            var points = GetForEachPoint(size, x => x);
            return points.Where(function);
        }

        public void DoForEachPoint(MazeSize size, Action<MazePoint> action)
        {
            foreach (var point in GetForEachPoint(size, x => x))
            {
                action(point);
            }
        }

        public void DoForEachZ(MazeSize size, int z, Action<MazePoint> action)
        {
            foreach (var point in GetForEachZ(size, z, x => x))
            {
                action(point);
            }
        }
    }
}

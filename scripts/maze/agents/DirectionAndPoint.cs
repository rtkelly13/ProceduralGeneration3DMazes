using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Agents
{
    public class DirectionAndPoint
    {
        private sealed class DirectionMazePointEqualityComparer : IEqualityComparer<DirectionAndPoint>
        {
            public bool Equals(DirectionAndPoint? x, DirectionAndPoint? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null) return false;
                if (y is null) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Direction == y.Direction && Equals(x.MazePoint, y.MazePoint);
            }

            public int GetHashCode(DirectionAndPoint obj)
            {
                unchecked
                {
                    return ((int)obj.Direction * 397) ^ (obj.MazePoint != null ? obj.MazePoint.GetHashCode() : 0);
                }
            }
        }

        private static readonly IEqualityComparer<DirectionAndPoint> DirectionMazePointComparerInstance = new DirectionMazePointEqualityComparer();

        public static IEqualityComparer<DirectionAndPoint> DirectionMazePointComparer => DirectionMazePointComparerInstance;

        public Direction Direction { get; set; }
        public MazePoint MazePoint { get; set; } = null!;
    }
}

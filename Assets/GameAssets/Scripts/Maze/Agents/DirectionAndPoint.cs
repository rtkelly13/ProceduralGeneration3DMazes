using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Agents
{
    public class DirectionAndPoint
    {
        private sealed class DirectionMazePointEqualityComparer : IEqualityComparer<DirectionAndPoint>
        {
            public bool Equals(DirectionAndPoint x, DirectionAndPoint y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Direction == y.Direction && Equals(x.MazePoint, y.MazePoint);
            }

            public int GetHashCode(DirectionAndPoint obj)
            {
                unchecked
                {
                    return ((int) obj.Direction*397) ^ (obj.MazePoint != null ? obj.MazePoint.GetHashCode() : 0);
                }
            }
        }

        private static readonly IEqualityComparer<DirectionAndPoint> DirectionMazePointComparerInstance = new DirectionMazePointEqualityComparer();

        public static IEqualityComparer<DirectionAndPoint> DirectionMazePointComparer
        {
            get { return DirectionMazePointComparerInstance; }
        }

        public Direction Direction { get; set; }
        public MazePoint MazePoint { get; set; }
    }
}
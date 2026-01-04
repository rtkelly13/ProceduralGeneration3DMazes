using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    /// <summary>
    /// Represents a complete path through the maze from start to end.
    /// </summary>
    public class PathResult
    {
        /// <summary>
        /// Index of this path (0 = optimal/shortest, 1+ = alternatives).
        /// </summary>
        public int PathIndex { get; set; }

        /// <summary>
        /// Total distance (number of steps) for this path.
        /// </summary>
        public int TotalDistance { get; set; }

        /// <summary>
        /// Difference in distance from the optimal path.
        /// </summary>
        public int DistanceFromOptimal { get; set; }

        /// <summary>
        /// Sequence of directions to traverse this path.
        /// </summary>
        public List<Direction> Directions { get; set; } = new();

        /// <summary>
        /// Junction nodes along this path (for graph view rendering).
        /// </summary>
        public List<MazePoint> JunctionNodes { get; set; } = new();

        /// <summary>
        /// Directed edges (from, to) along this path in the graph.
        /// </summary>
        public HashSet<(MazePoint From, MazePoint To)> PathEdges { get; set; } = new();

        /// <summary>
        /// Creates a PathResult from a ShortestPathResult.
        /// </summary>
        public static PathResult FromShortestPathResult(ShortestPathResult result, Graph graph, MazePoint startPoint, int pathIndex = 0)
        {
            var pathResult = new PathResult
            {
                PathIndex = pathIndex,
                TotalDistance = result.ShortestPath,
                DistanceFromOptimal = 0,
                Directions = new List<Direction>(result.ShortestPathDirections)
            };

            // Build junction nodes and path edges
            var currentPoint = startPoint;
            MazePoint? lastJunction = graph.Nodes.ContainsKey(startPoint) ? startPoint : null;

            if (lastJunction != null)
            {
                pathResult.JunctionNodes.Add(startPoint);
            }

            foreach (var direction in result.ShortestPathDirections)
            {
                currentPoint = MovePoint(currentPoint, direction);

                if (graph.Nodes.ContainsKey(currentPoint))
                {
                    pathResult.JunctionNodes.Add(currentPoint);

                    if (lastJunction != null)
                    {
                        pathResult.PathEdges.Add((lastJunction, currentPoint));
                    }
                    lastJunction = currentPoint;
                }
            }

            return pathResult;
        }

        private static MazePoint MovePoint(MazePoint point, Direction direction)
        {
            return direction switch
            {
                Direction.Left => new MazePoint(point.X - 1, point.Y, point.Z),
                Direction.Right => new MazePoint(point.X + 1, point.Y, point.Z),
                Direction.Forward => new MazePoint(point.X, point.Y + 1, point.Z),
                Direction.Back => new MazePoint(point.X, point.Y - 1, point.Z),
                Direction.Up => new MazePoint(point.X, point.Y, point.Z + 1),
                Direction.Down => new MazePoint(point.X, point.Y, point.Z - 1),
                _ => point
            };
        }
    }
}

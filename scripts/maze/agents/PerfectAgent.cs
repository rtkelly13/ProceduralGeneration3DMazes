using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Agents
{
    public class PerfectAgent : AgentBase
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public PerfectAgent(IDirectionsFlagParser directionsFlagParser)
        {
            _directionsFlagParser = directionsFlagParser;
        }

        public override AgentResults RunAgentBase(IMaze maze)
        {
            return new AgentResults
            {
                Movements = GetPathToLastPoint(new List<DirectionAndPoint>(), maze)
                    .Where(x => x.Direction != Direction.None).ToList()
            };
        }

        private List<DirectionAndPoint> GetPathToLastPoint(List<DirectionAndPoint> previousPoints,
            IMaze maze)
        {
            // If we are at the end return a list with the final Point added with No direction
            if (maze.CurrentPoint.Equals(maze.EndPoint))
            {
                previousPoints.Add(new DirectionAndPoint { Direction = Direction.None, MazePoint = maze.CurrentPoint });
                return previousPoints;
            }
            // If the cell has been visited immediately return the current List of previous points
            if (previousPoints.Any(x => x.MazePoint.Equals(maze.CurrentPoint)))
            {
                return previousPoints;
            }
            var directions = maze.GetDirectionsFromPoint();
            Random.Shared.Shuffle(directions);
            var currentPoint = maze.CurrentPoint;
            // Check each direction for path to end
            foreach (var direction in directions)
            {
                maze.MoveInDirection(direction);
                var newPreviousPoints = previousPoints.Concat(
                [
                    new DirectionAndPoint
                    {
                        Direction = direction,
                        MazePoint = currentPoint
                    }
                ]).ToList();
                var path = GetPathToLastPoint(newPreviousPoints, maze);
                var last = path.Last();
                if (last.MazePoint.Equals(maze.EndPoint))
                {
                    return path;
                }
                // Move back to start point if no path is found.
                maze.MoveInDirection(_directionsFlagParser.OppositeDirection(direction));
            }
            // If no path in any direction reaches end then return previous points.
            return previousPoints;
        }
    }
}

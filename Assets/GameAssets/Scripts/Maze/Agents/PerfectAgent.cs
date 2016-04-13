using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Agents
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
            //If we are at the end return a list with the final Point added with No direction
            if (maze.CurrentPoint.Equals(maze.EndPoint))
            {
                previousPoints.Add(new DirectionAndPoint { Direction = Direction.None, MazePoint = maze.CurrentPoint });
                return previousPoints;
            }
            //If the cell has been visited immediately return the current List of previous points
            if (previousPoints.Any(x => x.MazePoint.Equals(maze.CurrentPoint)))
            {
                return previousPoints;
            }
            var directions = maze.GetsDirectionsFromPoint().ToList();
            directions.Shuffle();
            var currentPoint = maze.CurrentPoint;
            //check each direction for path to end
            foreach (var direction in directions)
            {
                maze.MoveInDirection(direction);
                var newPreviousPoints = previousPoints.Concat(new[]
                {
                    new DirectionAndPoint
                    {
                        Direction = direction,
                        MazePoint = currentPoint
                    }
                }).ToList();
                var path = GetPathToLastPoint(newPreviousPoints, maze);
                var last = path.Last();
                if (last.MazePoint.Equals(maze.EndPoint))
                {
                    return path;
                }
                //Move back to start point if no path is found.
                maze.MoveInDirection(_directionsFlagParser.OppositeDirection(direction));
            }
            //If no path in any direction reaches end then return previous points.
            return previousPoints;
        }  
    }
}
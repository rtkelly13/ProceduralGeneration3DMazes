using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Agents
{
    /// <summary>
    /// An agent that explores the maze by randomly choosing directions,
    /// while avoiding going back the way it just came (except at dead ends).
    /// </summary>
    public class RandomAgent : AgentBase
    {
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public RandomAgent(IPointsAndDirectionsRetriever pointsAndDirectionsRetriever, IDirectionsFlagParser directionsFlagParser)
        {
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
            _directionsFlagParser = directionsFlagParser;
        }

        public override AgentResults RunAgentBase(IMaze maze)
        {
            var pointAndDirectionsList = new LinkedList<DirectionAndPoint>();
            if (!maze.CurrentPoint.Equals(maze.EndPoint))
            {
                var firstDirections = maze.GetDirectionsFromPoint();
                Random.Shared.Shuffle(firstDirections);
                var first = firstDirections[0];
                var currentPoint = maze.CurrentPoint;
                maze.MoveInDirection(first);
                var lastDirectionMoved = first;
                pointAndDirectionsList.AddLast(new DirectionAndPoint { MazePoint = currentPoint, Direction = first });
                while (!maze.CurrentPoint.Equals(maze.EndPoint))
                {
                    var directions = maze.GetDirectionsFromPoint();
                    var reverseDirection = _directionsFlagParser.OppositeDirection(lastDirectionMoved);
                    var filteredDirections = directions.Where(x => x != reverseDirection).ToArray();
                    Random.Shared.Shuffle(filteredDirections);
                    if (_pointsAndDirectionsRetriever.IsJunction(directions))
                    {
                        var direction = filteredDirections[0];
                        pointAndDirectionsList.AddLast(new DirectionAndPoint
                        {
                            Direction = direction,
                            MazePoint = maze.CurrentPoint
                        });
                        maze.MoveInDirection(direction);
                        lastDirectionMoved = direction;
                    }
                    else
                    {
                        if (filteredDirections.Length > 0)
                        {
                            var direction = filteredDirections[0];
                            pointAndDirectionsList.AddLast(new DirectionAndPoint
                            {
                                Direction = direction,
                                MazePoint = maze.CurrentPoint
                            });
                            maze.MoveInDirection(direction);
                            lastDirectionMoved = direction;
                        }
                        else
                        {
                            var direction = reverseDirection;
                            pointAndDirectionsList.AddLast(new DirectionAndPoint
                            {
                                Direction = direction,
                                MazePoint = maze.CurrentPoint
                            });
                            maze.MoveInDirection(direction);
                            lastDirectionMoved = direction;
                        }
                    }
                }
            }
            var list = pointAndDirectionsList.ToList();
            return new AgentResults
            {
                Movements = list.Distinct(DirectionAndPoint.DirectionMazePointComparer).ToList()
            };
        }
    }
}

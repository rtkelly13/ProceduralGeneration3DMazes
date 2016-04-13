using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Agents
{
    public class RandomAgent : AgentBase 
    {
        public override AgentResults RunAgentBase(IMaze maze)
        {
            var pointAndDirectionsList = new List<DirectionAndPoint>();
            while (!maze.CurrentPoint.Equals(maze.EndPoint))
            {
                var directions = maze.GetsDirectionsFromPoint().ToList();
                directions.Shuffle();
                var first = directions.First();
                pointAndDirectionsList.Add(new DirectionAndPoint { Direction = first, MazePoint = maze.CurrentPoint });
                maze.MoveInDirection(first);
            }
            var reducedList = pointAndDirectionsList.Distinct(DirectionAndPoint.DirectionMazePointComparer).ToList();
            return new AgentResults
            {
                Movements = reducedList
            };
        }
    }

    public class RandomAgent2: AgentBase
    {
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public RandomAgent2(IPointsAndDirectionsRetriever pointsAndDirectionsRetriever, IDirectionsFlagParser directionsFlagParser)
        {
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
            _directionsFlagParser = directionsFlagParser;
        }

        public override AgentResults RunAgentBase(IMaze maze)
        {
            var pointAndDirectionsList = new LinkedList<DirectionAndPoint>();
            if (!maze.CurrentPoint.Equals(maze.EndPoint))
            {
                var firstDirections = maze.GetsDirectionsFromPoint().ToList();
                firstDirections.Shuffle();
                var first = firstDirections.First();
                var currentPoint = maze.CurrentPoint;
                maze.MoveInDirection(first);
                var lastDirectionMoved = first;
                pointAndDirectionsList.AddLast(new DirectionAndPoint {MazePoint = currentPoint, Direction = first});
                while (!maze.CurrentPoint.Equals(maze.EndPoint))
                {
                    var directions = maze.GetsDirectionsFromPoint().ToList();
                    var reverseDirection = _directionsFlagParser.OppositeDirection(lastDirectionMoved);
                    var filteredDirections = directions.Where(x => x != reverseDirection).ToList();
                    filteredDirections.Shuffle();
                    if (_pointsAndDirectionsRetriever.IsJunction(directions))
                    {
                        var direction = filteredDirections.First();
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
                        if (filteredDirections.Any())
                        {
                            var direction = filteredDirections.First();
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

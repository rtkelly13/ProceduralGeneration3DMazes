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
        private readonly IArrayHelper _arrayHelper;

        public RandomAgent(IArrayHelper arrayHelper)
        {
            _arrayHelper = arrayHelper;
        }

        public override AgentResults RunAgentBase(IMaze maze)
        {
            var pointAndDirectionsList = new List<DirectionAndPoint>();
            while (!maze.CurrentPoint.Equals(maze.EndPoint))
            {
                var directions = maze.GetsDirectionsFromPoint().ToList();
                _arrayHelper.Shuffle(directions);
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
        private readonly IArrayHelper _arrayHelper;

        public RandomAgent2(IPointsAndDirectionsRetriever pointsAndDirectionsRetriever, IDirectionsFlagParser directionsFlagParser, IArrayHelper arrayHelper)
        {
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
            _directionsFlagParser = directionsFlagParser;
            _arrayHelper = arrayHelper;
        }

        public override AgentResults RunAgentBase(IMaze maze)
        {
            var pointAndDirectionsList = new LinkedList<DirectionAndPoint>();
            if (!maze.CurrentPoint.Equals(maze.EndPoint))
            {
                var firstDirections = maze.GetsDirectionsFromPoint().ToList();
                _arrayHelper.Shuffle(firstDirections);
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
                    _arrayHelper.Shuffle(filteredDirections);
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

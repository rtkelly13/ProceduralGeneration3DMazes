using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class DeadEndFillerRecursive : IDeadEndFiller
    {
        private readonly IDeadEndModelWrapperFactory _deadEndModelWrapperFactory;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;

        public DeadEndFillerRecursive(IDeadEndModelWrapperFactory deadEndModelWrapperFactory, IPointsAndDirectionsRetriever pointsAndDirectionsRetriever)
        {
            _deadEndModelWrapperFactory = deadEndModelWrapperFactory;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
        }

        public DeadEndFillerResult Fill(IMazeCarver mazeCarver)
        {
            mazeCarver.DoDeadEndWrapping(model =>
            {
                var deadEndModel = _deadEndModelWrapperFactory.MakeModel(model);
                return deadEndModel;
            });
            mazeCarver.SetState(ModelMode.DeadEndFilled);
            var deadEndsRemaining = true;
            var carvedDirections = new List<CarvedCellResult>();
            while (deadEndsRemaining)
            {
                var pointsAndDirections = _pointsAndDirectionsRetriever
                        .GetDeadEnds(mazeCarver)
                        .Where(x => !_pointsAndDirectionsRetriever.PointIsStartOrEnd(x.Point, mazeCarver.StartPoint, mazeCarver.EndPoint)).ToList();
                if (!pointsAndDirections.Any())
                {
                    deadEndsRemaining = false;
                }
                else
                {
                    foreach (var pointAndDirection in pointsAndDirections)
                    {
                        mazeCarver.JumpToPoint(pointAndDirection.Point);
                        carvedDirections.AddRange(FillInPassage(mazeCarver));
                    }
                }
            }
            
            mazeCarver.SetState(ModelMode.Standard);
            return new DeadEndFillerResult
            {
                CellsFilledIn = carvedDirections,
                TotalCellsFilledIn = carvedDirections.Count
            };
        }

        private IEnumerable<CarvedCellResult> FillInPassage(IMazeCarver mazeCarver)
        {
            var directions = mazeCarver.GetsDirectionsFromPoint().ToList();
            if (directions.Count() == 1)
            {
                var direction = directions.First();
                mazeCarver.FillInDirection(direction);
                yield return new CarvedCellResult
                {
                    Direction = direction,
                    Point = mazeCarver.CurrentPoint
                };
                mazeCarver.JumpInDirection(direction);
               
                if (!_pointsAndDirectionsRetriever.PointIsStartOrEnd(mazeCarver.CurrentPoint, mazeCarver.StartPoint, mazeCarver.EndPoint))
                {
                    foreach (var d in FillInPassage(mazeCarver))
                    {
                        yield return d;
                    }
                }
            }
        }
    }

    public class DeadEndFiller : IDeadEndFiller
    {
        private readonly IDeadEndModelWrapperFactory _deadEndModelWrapperFactory;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;

        public DeadEndFiller(IDeadEndModelWrapperFactory deadEndModelWrapperFactory, IPointsAndDirectionsRetriever pointsAndDirectionsRetriever)
        {
            _deadEndModelWrapperFactory = deadEndModelWrapperFactory;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
        }

        public DeadEndFillerResult Fill(IMazeCarver mazeCarver)
        {
            mazeCarver.DoDeadEndWrapping(model =>
            {
                var deadEndModel = _deadEndModelWrapperFactory.MakeModel(model);
                return deadEndModel;
            });
            mazeCarver.SetState(ModelMode.DeadEndFilled);
            var deadEndsRemaining = true;
            var carvedDirections = new List<CarvedCellResult>();
            while (deadEndsRemaining)
            {
                var pointsAndDirections = _pointsAndDirectionsRetriever
                        .GetDeadEnds(mazeCarver)
                        .Where(x => !_pointsAndDirectionsRetriever.PointIsStartOrEnd(x.Point, mazeCarver.StartPoint, mazeCarver.EndPoint)).ToList();
                if (!pointsAndDirections.Any())
                {
                    deadEndsRemaining = false;
                }
                else
                {
                    foreach (var pointAndDirection in pointsAndDirections)
                    {
                        mazeCarver.JumpToPoint(pointAndDirection.Point);
                        carvedDirections.AddRange(FillInPassage(mazeCarver));
                    }
                }
            }

            mazeCarver.SetState(ModelMode.Standard);
            return new DeadEndFillerResult
            {
                CellsFilledIn = carvedDirections,
                TotalCellsFilledIn = carvedDirections.Count
            };
        }

        private IEnumerable<CarvedCellResult> FillInPassage(IMazeCarver mazeCarver)
        {
            var list = new List<CarvedCellResult>();

            var junctionOrStartOrEndReached = false;
            do
            {
                var directions = mazeCarver.GetsDirectionsFromPoint().ToList();
                if (directions.Count() == 1)
                {
                    var direction = directions.First();
                    mazeCarver.FillInDirection(direction);
                    list.Add(new CarvedCellResult
                    {
                        Direction = direction,
                        Point = mazeCarver.CurrentPoint
                    });
                    mazeCarver.JumpInDirection(direction);

                    if (_pointsAndDirectionsRetriever.PointIsStartOrEnd(mazeCarver.CurrentPoint, mazeCarver.StartPoint, mazeCarver.EndPoint))
                    {
                        junctionOrStartOrEndReached = true;
                    }
                }
                else
                {
                    junctionOrStartOrEndReached = true;
                }

            } while (!junctionOrStartOrEndReached);
            return list;
        }
    }
}

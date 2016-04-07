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
    public class DeadEndFiller : IDeadEndFiller
    {
        private readonly IDeadEndModelWrapperFactory _deadEndModelWrapperFactory;
        private readonly IDeadEndRetriever _deadEndRetriever;

        public DeadEndFiller(IDeadEndModelWrapperFactory deadEndModelWrapperFactory, IDeadEndRetriever deadEndRetriever)
        {
            _deadEndModelWrapperFactory = deadEndModelWrapperFactory;
            _deadEndRetriever = deadEndRetriever;
        }

        public DeadEndFillerResult Fill(IMazeCarver mazeCarver)
        {
            mazeCarver.DoDeadEndWrapping(model =>
            {
                var deadEndModel = _deadEndModelWrapperFactory.MakeModel(model);
                return deadEndModel;
            });
            mazeCarver.ToggleDeadEnd();
            var deadEndsRemaining = true;
            var carvedDirections = new List<CarvedCellResult>();
            while (deadEndsRemaining)
            {
                var points = _deadEndRetriever
                        .GetDeadEnds(mazeCarver)
                        .Where(x => !PointIsStartOrEnd(x, mazeCarver)).ToList();
                if (!points.Any())
                {
                    deadEndsRemaining = false;
                }
                else
                {
                    foreach (var point in points)
                    {
                        mazeCarver.JumpToPoint(point);
                        carvedDirections.AddRange(FillInPassage(mazeCarver));
                    }
                }
            }
            
            mazeCarver.ToggleDeadEnd();
            return new DeadEndFillerResult
            {
                CellsFilledIn = carvedDirections.Count
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
               
                if (!PointIsStartOrEnd(mazeCarver.CurrentPoint, mazeCarver))
                {
                    foreach (var d in FillInPassage(mazeCarver))
                    {
                        yield return d;
                    }
                }
            }
        }

        private bool PointIsStartOrEnd(MazePoint point, IMazeCarver mazeCarver)
        {
            return point.Equals(mazeCarver.StartPoint) || point.Equals(mazeCarver.EndPoint);
        }
    }
}

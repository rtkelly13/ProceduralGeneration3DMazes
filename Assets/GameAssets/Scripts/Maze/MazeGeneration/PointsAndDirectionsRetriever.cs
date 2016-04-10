using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class PointsAndDirectionsRetriever : IPointsAndDirectionsRetriever
    {
        private readonly IMazeHelper _mazeHelper;

        public PointsAndDirectionsRetriever(IMazeHelper mazeHelper)
        {
            _mazeHelper = mazeHelper;
        }

        public IEnumerable<PointAndDirections> GetDeadEnds(IMazeCarver mazeCarver)
        {
            return GetPointsWithDirections(mazeCarver).Where(x => IsDeadEnd(x.Directions));
        }

        public bool IsDeadEnd(IEnumerable<Direction> directions)
        {
            return directions.Count() == 1;
        }

        public IEnumerable<PointAndDirections> GetCorridoors(IMazeCarver mazeCarver)
        {
            return GetPointsWithDirections(mazeCarver).Where(x => IsCorridoor(x.Directions));
        }

        public bool IsCorridoor(IEnumerable<Direction> directions)
        {
            return directions.Count() == 2;
        }

        public IEnumerable<PointAndDirections> GetJunctions(IMazeCarver mazeCarver)
        {
            return GetPointsWithDirections(mazeCarver).Where(x => IsJunction(x.Directions));
        }

        public bool IsJunction(IEnumerable<Direction> directions)
        {
            return directions.Count() > 2;
        }

        public bool PointIsStartOrEnd(MazePoint point, MazePoint start, MazePoint end)
        {
            return point.Equals(start) || point.Equals(end);
        }

        private IEnumerable<PointAndDirections> GetPointsWithDirections(IMazeCarver carver)
        {
            return _mazeHelper.GetForEachPoint(carver.Size, point =>
            {
                carver.JumpToPoint(point);
                return new PointAndDirections
                {
                    Point = point,
                    Directions = carver.GetsDirectionsFromPoint().ToList()
                };
            });
        } 
    }
}

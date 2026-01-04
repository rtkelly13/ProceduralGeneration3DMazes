using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public class PointsAndDirectionsRetriever : IPointsAndDirectionsRetriever
    {
        private readonly IMazeHelper _mazeHelper;

        public PointsAndDirectionsRetriever(IMazeHelper mazeHelper)
        {
            _mazeHelper = mazeHelper;
        }

        public IEnumerable<PointAndDirections> GetDeadEnds(IMazeJumper mazeJumper)
        {
            return GetPointsWithDirections(mazeJumper).Where(x => IsDeadEnd(x.Directions));
        }

        public bool IsDeadEnd(IEnumerable<Direction> directions)
        {
            return directions.Count() == 1;
        }

        public IEnumerable<PointAndDirections> GetCorridoors(IMazeJumper mazeJumper)
        {
            return GetPointsWithDirections(mazeJumper).Where(x => IsCorridoor(x.Directions));
        }

        public bool IsCorridoor(IEnumerable<Direction> directions)
        {
            return directions.Count() == 2;
        }

        public IEnumerable<PointAndDirections> GetJunctions(IMazeJumper mazeJumper)
        {
            return GetPointsWithDirections(mazeJumper).Where(x => IsJunction(x.Directions));
        }

        public bool IsJunction(IEnumerable<Direction> directions)
        {
            return directions.Count() > 2;
        }

        public bool PointIsStartOrEnd(MazePoint point, MazePoint start, MazePoint end)
        {
            return point.Equals(start) || point.Equals(end);
        }

        private IEnumerable<PointAndDirections> GetPointsWithDirections(IMazeJumper jumper)
        {
            return _mazeHelper.GetForEachPoint(jumper.Size, point =>
            {
                jumper.JumpToPoint(point);
                return new PointAndDirections
                {
                    Point = point,
                    Directions = jumper.GetDirectionsFromPoint()
                };
            });
        }
    }
}

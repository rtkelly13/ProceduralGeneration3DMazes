using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public interface IPointsAndDirectionsRetriever
    {
        IEnumerable<PointAndDirections> GetCorridoors(IMazeJumper mazeJumper);
        IEnumerable<PointAndDirections> GetDeadEnds(IMazeJumper mazeJumper);
        IEnumerable<PointAndDirections> GetJunctions(IMazeJumper mazeJumper);
        bool IsCorridoor(IEnumerable<Direction> directions);
        bool IsDeadEnd(IEnumerable<Direction> directions);
        bool IsJunction(IEnumerable<Direction> directions);
        bool PointIsStartOrEnd(MazePoint point, MazePoint start, MazePoint end);
    }
}

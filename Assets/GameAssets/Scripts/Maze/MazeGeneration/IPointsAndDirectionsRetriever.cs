using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public interface IPointsAndDirectionsRetriever
    {
        IEnumerable<PointAndDirections> GetCorridoors(IMazeCarver mazeCarver);
        IEnumerable<PointAndDirections> GetDeadEnds(IMazeCarver mazeCarver);
        IEnumerable<PointAndDirections> GetJunctions(IMazeCarver mazeCarver);
        bool IsCorridoor(IEnumerable<Direction> directions);
        bool IsDeadEnd(IEnumerable<Direction> directions);
        bool IsJunction(IEnumerable<Direction> directions);
        bool PointIsStartOrEnd(MazePoint point, MazePoint start, MazePoint end);
    }
}
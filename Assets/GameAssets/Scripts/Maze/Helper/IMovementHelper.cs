using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public interface IMovementHelper
    {
        IEnumerable<Direction> AdjacentPoints(MazePoint p, MazeSize size);
        Direction AdjacentPointsFlag(MazePoint p, MazeSize size);
        MazePoint Move(MazePoint start, Direction d, MazeSize size);
        bool CanMove(MazePoint start, Direction d, MazeSize size, out MazePoint final);
        MazePoint Move(MazePoint start, MazePoint final, MazeSize size);
        bool CanMove(MazePoint start, MazePoint final, MazeSize size);
    }
}
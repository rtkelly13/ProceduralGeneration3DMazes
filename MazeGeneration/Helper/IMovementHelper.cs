using System.Collections.Generic;
using MazeGeneration.Factory;
using MazeGeneration.Model;

namespace MazeGeneration.Helper
{
    public interface IMovementHelper
    {
        IEnumerable<Direction> AdjacentPoints(MazePoint p, MazeSize size);
        Direction AdjacentPointsFlag(MazePoint p, MazeSize size);
        MazePoint Move(MazePoint start, Direction d, MazeSize size);
        bool CanMove(MazePoint start, Direction d, MazeSize size, out MazePoint final);
        bool ValidPoint(MazePoint p, MazeSize size);
    }
}
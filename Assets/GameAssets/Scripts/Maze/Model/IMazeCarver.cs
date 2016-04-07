using System.Collections.Generic;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public interface IMazeCarver: IMazeJumper
    {
        IEnumerable<Direction> CarvableDirections();
        Direction CarvableFlag();

        IEnumerable<Direction> AlreadyCarvedDirections();
        Direction AlreadyCarvedFlag();

        bool CanCarveInDirection(Direction d);
        void CarveInDirection(Direction d);
        void FillInDirection(Direction d);
        bool AlreadyCarvedDirection(Direction d);

        IMazeJumper CarvingFinished();
    }
}

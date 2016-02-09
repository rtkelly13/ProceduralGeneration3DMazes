using System.Collections.Generic;

namespace Assets.GameAssets.Scripts.Maze
{
    public interface IMazeCarver: IMazeJumper
    {
        IEnumerable<Direction> CarvableDirections();
        Direction CarvableFlag();

        IEnumerable<Direction> AlreadyCarvedDirections();
        Direction AlreadyCarvedFlag();

        bool CanCarveInDirection(Direction d);
        void CarveInDirection(Direction d);
        bool AlreadyCarvedDirection(Direction d);

        IMazeJumper CarvingFinished();
    }
}

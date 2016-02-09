using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public interface IMazeJumper: IMaze
    {
        IEnumerable<Direction> JumpableDirections();
        Direction JumpableFlag();

        bool CanJumpInDirection(Direction d);
        bool TryJumpInDirection(Direction d);
        void JumpInDirection(Direction d);

        bool CanJumpToPoint(MazePoint p);
        bool TryJumpToPoint(MazePoint p);
        void JumpToPoint(MazePoint p);

        IMaze JumpingFinished();
    }
}

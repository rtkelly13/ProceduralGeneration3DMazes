using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public interface IMaze
    {
        Direction GetFlagFromPoint();
        IEnumerable<Direction> GetsDirectionsFromPoint();

        bool HasVertexes(Direction flag);
        void MoveInDirection(Direction d);

        MazePoint CurrentPoint { get; }
        MazeSize Size { get; }
    }
}

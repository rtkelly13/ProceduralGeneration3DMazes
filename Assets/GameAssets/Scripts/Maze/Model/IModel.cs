using System.Collections;
using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public interface IModel
    {
        bool HasDirections(MazePoint p, Direction d);
        Direction GetFlagFromPoint(MazePoint p);

        MazeSize Size { get; }
        MazePoint StartPoint { get; }
        MazePoint EndPoint { get; }
    }
}

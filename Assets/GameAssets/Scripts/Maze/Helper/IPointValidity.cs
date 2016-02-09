using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public interface IPointValidity
    {
        bool ValidPoint(MazePoint p, MazeSize size);
    }
}
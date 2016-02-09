using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public interface IRandomPointGenerator
    {
        MazePoint RandomPoint(MazeSize size);
    }
}
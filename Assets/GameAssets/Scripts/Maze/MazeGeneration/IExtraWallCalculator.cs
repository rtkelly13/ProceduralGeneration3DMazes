using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public interface IExtraWallCalculator
    {
        int Calulate(MazeSize size);
    }
}
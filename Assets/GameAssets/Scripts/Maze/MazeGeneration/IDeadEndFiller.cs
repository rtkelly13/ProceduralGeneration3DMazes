using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public interface IDeadEndFiller
    {
        DeadEndFillerResult Fill(IMazeCarver mazeCarver);
    }
}
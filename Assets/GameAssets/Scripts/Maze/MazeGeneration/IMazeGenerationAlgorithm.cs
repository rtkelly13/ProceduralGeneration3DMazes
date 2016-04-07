using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public interface IMazeGenerationAlgorithm
    {
        IMazeCarver GenerateMaze(IMazeCarver initialisedMaze, MazeGenerationSettings settings);
    }
}
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public interface IMazeModelFactory
    {
        IModelBuilder BuildMaze(MazeGenerationSettings settings);
    }
}
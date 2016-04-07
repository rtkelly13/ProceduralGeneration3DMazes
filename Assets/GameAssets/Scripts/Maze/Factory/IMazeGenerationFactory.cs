using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public interface IMazeGenerationFactory
    {
        IMazeJumper GenerateMaze(MazeGenerationSettings settings);
    }
}

using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public interface IMazeArrayBuilder
    {
        MazeCell[,,] Build(MazeSize size);
    }
}
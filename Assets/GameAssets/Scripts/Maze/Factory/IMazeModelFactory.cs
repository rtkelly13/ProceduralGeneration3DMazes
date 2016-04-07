using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public interface IMazeModelFactory
    {
        ModelBase BuildMaze(MazeType type, MazeSize size);
    }
}
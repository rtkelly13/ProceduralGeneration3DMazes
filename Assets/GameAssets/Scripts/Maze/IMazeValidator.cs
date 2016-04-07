using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public interface IMazeValidator
    {
        bool EveryPointHasDirection(IMazeJumper maze);
    }
}
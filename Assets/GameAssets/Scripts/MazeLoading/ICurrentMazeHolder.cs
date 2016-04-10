using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeLoading
{
    public interface ICurrentMazeHolder
    {
        MazeGenerationResults Results { get; set; }
    }
}
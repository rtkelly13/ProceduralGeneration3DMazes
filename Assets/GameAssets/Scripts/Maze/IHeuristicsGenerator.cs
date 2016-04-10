using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public interface IHeuristicsGenerator
    {
        HeuristicsResults GetResults( IMazeCarver carver);
    }
}
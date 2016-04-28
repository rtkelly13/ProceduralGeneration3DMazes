using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
{
    public interface IHeuristicsGenerator
    {
        HeuristicsResults GetResults(AlgorithmRunResults carver);
    }
}
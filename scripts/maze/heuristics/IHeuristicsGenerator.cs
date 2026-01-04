using ProceduralMaze.Maze.Generation;

namespace ProceduralMaze.Maze.Heuristics
{
    public interface IHeuristicsGenerator
    {
        HeuristicsResults GetResults(AlgorithmRunResults results);
    }
}

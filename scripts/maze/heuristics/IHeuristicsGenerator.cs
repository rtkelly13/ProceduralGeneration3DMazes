using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Heuristics
{
    public interface IHeuristicsGenerator
    {
        HeuristicsResults GetResults(AlgorithmRunResults results, MazeGenerationSettings settings);
    }
}

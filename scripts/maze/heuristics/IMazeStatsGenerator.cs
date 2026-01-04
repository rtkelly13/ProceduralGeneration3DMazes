using ProceduralMaze.Maze.Generation;

namespace ProceduralMaze.Maze.Heuristics
{
    public interface IMazeStatsGenerator
    {
        MazeStatsResult GetResultsFromMaze(AlgorithmRunResults results);
    }
}

using Assets.GameAssets.Scripts.Maze.MazeGeneration;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
{
    public interface IMazeStatsGenerator
    {
        MazeStatsResult GetResultsFromMaze(AlgorithmRunResults results);
    }
}
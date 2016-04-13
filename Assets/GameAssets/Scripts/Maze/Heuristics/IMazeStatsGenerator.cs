using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
{
    public interface IMazeStatsGenerator
    {
        MazeStatsResults GetResultsFromMaze(IMazeCarver mazeCarver);
    }
}
using Assets.GameAssets.Scripts.Maze.Solver;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
{
    public class HeuristicsResults
    {
        public int TotalCells { get; set; }
        public ShortestPathResult ShortestPathResult { get; set; }
        public MazeStatsResults MazeStats { get; set; }
    }
}
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.Maze.Heuristics
{
    public class HeuristicsResults
    {
        public int TotalCells { get; set; }
        public ShortestPathResult ShortestPathResult { get; set; } = null!;
        public MazeStatsResult Stats { get; set; } = null!;
    }
}

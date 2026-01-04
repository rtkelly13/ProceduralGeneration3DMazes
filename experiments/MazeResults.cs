using ProceduralMaze.Maze.Heuristics;

namespace ProceduralMaze.Experiments;

public class MazeResults
{
    public int ShortestPath { get; set; }
    public MazeStatsResult Stats { get; set; } = null!;
    public TimeSpan ModelTime { get; set; }
    public TimeSpan GenerationTime { get; set; }
    public TimeSpan DeadEndFillerTime { get; set; }
    public TimeSpan AgentGenerationTime { get; set; }
    public TimeSpan HeuristicsTime { get; set; }
    public TimeSpan TotalTime { get; set; }
    public int TotalCellsFilledIn { get; set; }
}

using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Experiments;

public class ExperimentResult
{
    public double AverageShortestPath { get; set; }
    public TimeSpan AverageGenerationTime { get; set; }
    public Dictionary<Direction, double> DirectionWeightings { get; set; } = new();
    public double AverageCellsFilledIn { get; set; }
}

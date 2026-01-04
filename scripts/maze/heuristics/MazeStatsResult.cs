using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Heuristics
{
    public class MazeStatsResult
    {
        public Dictionary<Direction, int> DirectionsUsed { get; set; } = new();
        public DirectionResult MaximumUse { get; set; } = null!;
        public DirectionResult MinimumUse { get; set; } = null!;
    }
}

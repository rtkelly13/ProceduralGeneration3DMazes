using System;
using System.Collections.Generic;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Heuristics;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public class MazeGenerationResults
    {
        public IMazeJumper MazeJumper { get; set; } = null!;
        public HeuristicsResults HeuristicsResults { get; set; } = null!;
        public DeadEndFillerResult DeadEndFillerResults { get; set; } = null!;
        public AgentResults? AgentResults { get; set; }
        public TimeSpan ModelTime { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public TimeSpan DeadEndFillerTime { get; set; }
        public TimeSpan AgentGenerationTime { get; set; }
        public TimeSpan HeuristicsTime { get; set; }
        public TimeSpan TotalTime { get; set; }
        public List<DirectionAndPoint> DirectionsCarvedIn { get; set; } = new();
    }
}

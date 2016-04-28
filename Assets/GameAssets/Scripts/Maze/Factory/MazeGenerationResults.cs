using System;
using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Heuristics;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class MazeGenerationResults
    {
        public IMazeJumper MazeJumper { get; set; }
        public HeuristicsResults HeuristicsResults { get; set; }
        public DeadEndFillerResult DeadEndFillerResults { get; set; }
        public AgentResults AgentResults { get; set; }
        public TimeSpan ModelTime { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public TimeSpan DeadEndFillerTime { get; set; }
        public TimeSpan AgentGenerationTime { get; set; }
        public TimeSpan HeuristicsTime { get; set; }
        public TimeSpan TotalTime { get; set; }
        public List<DirectionAndPoint> DirectionsCarvedIn { get; set; }
    }
}
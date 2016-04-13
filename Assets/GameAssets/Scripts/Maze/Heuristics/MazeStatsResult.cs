using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
{
    public class MazeStatsResult
    {
        public Dictionary<Direction, int> DirectionsUsed { get; set; }
        public DirectionResult MaximumUse { get; set; }
        public DirectionResult MinimumUse { get; set; }
    }
}
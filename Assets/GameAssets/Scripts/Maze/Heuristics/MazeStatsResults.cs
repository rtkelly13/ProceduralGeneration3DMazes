using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
{
    public class MazeStatsResults
    {
        public MazeStatsResult Standard { get; set; }
        public MazeStatsResult DeadEnd { get; set; }
    }
}
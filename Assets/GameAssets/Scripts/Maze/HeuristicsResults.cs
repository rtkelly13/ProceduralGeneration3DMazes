using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.Maze.Solver;

namespace Assets.GameAssets.Scripts.Maze
{
    public class HeuristicsResults
    {
        public int TotalCells { get; set; }
        public ShortestPathResult ShortestPathResult { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.Maze.Solver;

namespace Assets.GameAssets.Scripts.Maze
{
    public class HeuristicsGenerator : IHeuristicsGenerator
    {
        private readonly IShortestPathSolver _shortestPathSolver;

        public HeuristicsGenerator(IShortestPathSolver shortestPathSolver)
        {
            _shortestPathSolver = shortestPathSolver;
        }

        public HeuristicsResults GetResults(IMazeCarver carver)
        {
            return new HeuristicsResults
            {
                TotalCells = GetTotalSize(carver.Size),
                ShortestPathResult = _shortestPathSolver.GetGraph(carver)
            };
        }

        private int GetTotalSize(MazeSize size)
        {
            return size.X * size.Z * size.Z;
        }
    }
}

using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Solver
{
    public interface IShortestPathSolver
    {
        ShortestPathResult GetGraph(IMazeCarver carver);
    }
}
using System;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver.Heuristics
{
    public class EuclideanHeuristic : IHeuristicStrategy
    {
        public double Calculate(MazePoint current, MazePoint goal)
        {
            return Math.Sqrt(Math.Pow(current.X - goal.X, 2) + 
                             Math.Pow(current.Y - goal.Y, 2) + 
                             Math.Pow(current.Z - goal.Z, 2));
        }
    }
}

using System;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver.Heuristics
{
    public class ManhattanHeuristic : IHeuristicStrategy
    {
        public double Calculate(MazePoint current, MazePoint goal)
        {
            return Math.Abs(current.X - goal.X) + 
                   Math.Abs(current.Y - goal.Y) + 
                   Math.Abs(current.Z - goal.Z);
        }
    }
}

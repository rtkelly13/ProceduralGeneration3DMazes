using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver.Heuristics
{
    public interface IHeuristicStrategy
    {
        double Calculate(MazePoint current, MazePoint goal);
    }
}

using ProceduralMaze.Maze.Solver.Heuristics;

namespace ProceduralMaze.Maze.Solver
{
    public interface ISolverFactory
    {
        IShortestPathSolver CreateSolver(SolverType solverType, HeuristicType heuristicType);
    }
}

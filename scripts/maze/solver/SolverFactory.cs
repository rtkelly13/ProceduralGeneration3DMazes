using System;
using ProceduralMaze.Maze.Solver.Heuristics;

namespace ProceduralMaze.Maze.Solver
{
    public class SolverFactory : ISolverFactory
    {
        private readonly IGraphBuilder _graphBuilder;

        public SolverFactory(IGraphBuilder graphBuilder)
        {
            _graphBuilder = graphBuilder;
        }

        public IShortestPathSolver CreateSolver(SolverType solverType, HeuristicType heuristicType)
        {
            switch (solverType)
            {
                case SolverType.Dijkstra:
                    return new ShortestPathSolver(_graphBuilder);
                case SolverType.AStar:
                    return new AStarSolver(_graphBuilder, CreateHeuristic(heuristicType));
                default:
                    throw new ArgumentException($"Unsupported solver type: {solverType}");
            }
        }

        private IHeuristicStrategy CreateHeuristic(HeuristicType heuristicType)
        {
            switch (heuristicType)
            {
                case HeuristicType.Manhattan:
                    return new ManhattanHeuristic();
                case HeuristicType.Euclidean:
                    return new EuclideanHeuristic();
                default:
                    throw new ArgumentException($"Unsupported heuristic type: {heuristicType}");
            }
        }
    }
}

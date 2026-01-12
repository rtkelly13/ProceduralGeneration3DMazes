using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.Maze.Heuristics
{
    public class HeuristicsGenerator : IHeuristicsGenerator
    {
        private readonly ISolverFactory _solverFactory;
        private readonly IMazeStatsGenerator _mazeStatsGenerator;

        public HeuristicsGenerator(ISolverFactory solverFactory, IMazeStatsGenerator mazeStatsGenerator)
        {
            _solverFactory = solverFactory;
            _mazeStatsGenerator = mazeStatsGenerator;
        }

        public HeuristicsResults GetResults(AlgorithmRunResults results, MazeGenerationSettings settings)
        {
            var solver = _solverFactory.CreateSolver(settings.SolverType, settings.HeuristicType);
            var shortestPath = solver.GetGraph(results.Carver);

            return new HeuristicsResults
            {
                TotalCells = GetTotalSize(results.Carver.Size),
                ShortestPathResult = shortestPath,
                Stats = _mazeStatsGenerator.GetResultsFromMaze(results)
            };
        }

        private int GetTotalSize(MazeSize size)
        {
            return size.X * size.Y * size.Z;
        }
    }
}

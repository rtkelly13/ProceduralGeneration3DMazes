using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.Maze.Heuristics
{
    public class HeuristicsGenerator : IHeuristicsGenerator
    {
        private readonly IShortestPathSolver _shortestPathSolver;
        private readonly IMazeStatsGenerator _mazeStatsGenerator;

        public HeuristicsGenerator(IShortestPathSolver shortestPathSolver, IMazeStatsGenerator mazeStatsGenerator)
        {
            _shortestPathSolver = shortestPathSolver;
            _mazeStatsGenerator = mazeStatsGenerator;
        }

        public HeuristicsResults GetResults(AlgorithmRunResults results)
        {
            var shortestPath = _shortestPathSolver.GetGraph(results.Carver);
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

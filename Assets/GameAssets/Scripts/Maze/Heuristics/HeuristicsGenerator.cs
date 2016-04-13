using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.Maze.Solver;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
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

        public HeuristicsResults GetResults(IMazeCarver carver)
        {
            var shortestPath = _shortestPathSolver.GetGraph(carver);
            var stats = _mazeStatsGenerator.GetResultsFromMaze(carver);
            return new HeuristicsResults
            {
                TotalCells = GetTotalSize(carver.Size),
                ShortestPathResult = shortestPath,
                MazeStats = stats
            };
        }

        private int GetTotalSize(MazeSize size)
        {
            return size.X * size.Y * size.Z;
        }
    }
}

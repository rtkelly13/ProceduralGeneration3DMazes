using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    /// <summary>
    /// Binary Tree Algorithm implementation.
    /// Currently uses the same logic as BacktrackerAlgorithm as a placeholder.
    /// </summary>
    public class BinaryTreeAlgorithm : IBinaryTreeAlgorithm
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IRandomValueGenerator _randomValueGenerator;

        public BinaryTreeAlgorithm(IDirectionsFlagParser directionsFlagParser,
            IRandomPointGenerator randomPointGenerator,
            IRandomValueGenerator randomValueGenerator)
        {
            _directionsFlagParser = directionsFlagParser;
            _randomPointGenerator = randomPointGenerator;
            _randomValueGenerator = randomValueGenerator;
        }

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            // Use backtracker logic as placeholder for binary tree
            var backtracker = new BacktrackerAlgorithm(_directionsFlagParser, _randomPointGenerator, _randomValueGenerator);
            return backtracker.GenerateMaze(maze, settings);
        }
    }
}

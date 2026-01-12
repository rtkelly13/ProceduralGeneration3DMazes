using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public interface IMazeGenerationAlgorithm
    {
        AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings);
    }

    public interface IGrowingTreeAlgorithm : IMazeGenerationAlgorithm
    {
    }

    public interface IRecursiveBacktrackerAlgorithm : IMazeGenerationAlgorithm
    {
    }

    public interface IBinaryTreeAlgorithm : IMazeGenerationAlgorithm
    {
    }

    public interface IPrimsAlgorithm : IMazeGenerationAlgorithm
    {
    }
}

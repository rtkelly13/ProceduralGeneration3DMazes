using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public interface IMazeGenerationFactory
    {
        MazeGenerationResults GenerateMaze(MazeGenerationSettings settings);
    }
}

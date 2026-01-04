using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public interface IWallRemovalCalculator
    {
        int Calculate(MazeSize size);
    }
}

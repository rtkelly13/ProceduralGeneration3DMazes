using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public interface IRandomPointGenerator
    {
        MazePoint RandomPoint(MazeSize size, PickType type = PickType.Random);
    }
}

using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Helper
{
    public interface IPointValidity
    {
        bool ValidPoint(MazePoint p, MazeSize size);
    }
}

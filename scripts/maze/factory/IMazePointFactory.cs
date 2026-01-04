using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public interface IMazePointFactory
    {
        MazePoint MakePoint(int x, int y, int z);
    }

    public interface IMazePointPool : IMazePointFactory
    {
        void Store(MazePoint p);
    }
}

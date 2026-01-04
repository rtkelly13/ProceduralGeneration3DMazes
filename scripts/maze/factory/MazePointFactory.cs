using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public class MazePointFactory : IMazePointFactory
    {
        public MazePoint MakePoint(int x, int y, int z)
        {
            return new MazePoint(x, y, z);
        }
    }
}

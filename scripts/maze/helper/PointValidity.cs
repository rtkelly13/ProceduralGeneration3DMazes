using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Helper
{
    public class PointValidity : IPointValidity
    {
        public bool ValidPoint(MazePoint p, MazeSize size)
        {
            if (p.X < 0 || p.X > size.X - 1)
            {
                return false;
            }

            if (p.Y < 0 || p.Y > size.Y - 1)
            {
                return false;
            }

            if (p.Z < 0 || p.Z > size.Z - 1)
            {
                return false;
            }
            return true;
        }
    }
}

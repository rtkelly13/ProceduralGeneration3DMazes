using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public class PointAndDirections
    {
        public required MazePoint Point { get; set; }
        public required Direction[] Directions { get; set; }
    }
}

using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public class CarvedCellResult
    {
        public Direction Direction { get; set; }
        public MazePoint Point { get; set; } = null!;
    }
}

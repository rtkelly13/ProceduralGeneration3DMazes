using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    public class GraphEdge
    {
        public required MazePoint Point { get; set; }
        public required Direction[] DirectionsToPoint { get; set; }
    }
}

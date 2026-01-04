using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public class ModelInitialisationOptions
    {
        public required MazeSize Size { get; set; }
        public required MazePoint StartPoint { get; set; }
        public required MazePoint EndPoint { get; set; }
    }
}

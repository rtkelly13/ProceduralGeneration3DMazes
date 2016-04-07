using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class ModelInitialisationOptions
    {
        public MazeSize Size { get; set; } 
        public MazePoint StartPoint { get; set; }
        public MazePoint EndPoint { get; set; }
    }
}
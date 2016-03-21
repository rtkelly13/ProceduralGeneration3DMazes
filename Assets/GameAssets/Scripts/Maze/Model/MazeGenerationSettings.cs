using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.UI;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class MazeGenerationSettings
    {
        public Algorithm Algorithm { get; set; }
        public MazeSize Size { get; set; }
        public MazeType Option { get; set; }
    }
}

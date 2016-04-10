using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.UI;
using Assets.GameAssets.Scripts.UI.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class MazeGenerationSettings
    {
        public Algorithm Algorithm { get; set; }
        public MazeSize Size { get; set; }
        public MazeType Option { get; set; }
        public WallCarverOption ExtraWalls { get; set; }
        public bool DoorsAtEdge { get; set; }
    }
}

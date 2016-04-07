using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public class CellValidationResult
    {
        public bool CellValid { get; set; }
        public Direction Flag { get; set; }
        public MazePoint Point { get; set; }
    }
}
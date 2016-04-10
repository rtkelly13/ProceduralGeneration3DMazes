using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class MazeGenerationResults
    {
        public IMazeJumper MazeJumper { get; set; }
        public HeuristicsResults HeuristicsResults { get; set; }
        public DeadEndFillerResult DeadEndFillerResults { get; set; }
    }
}
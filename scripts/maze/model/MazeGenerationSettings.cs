using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;

namespace ProceduralMaze.Maze.Model
{
    public class MazeGenerationSettings
    {
        public Algorithm Algorithm { get; set; }
        public MazeSize Size { get; set; } = new MazeSize(10, 10, 1);
        public MazeType Option { get; set; }
        public double WallRemovalPercent { get; set; }
        public bool DoorsAtEdge { get; set; }
        public AgentType AgentType { get; set; }
        public GrowingTreeSettings GrowingTreeSettings { get; set; } = new GrowingTreeSettings();
    }
}

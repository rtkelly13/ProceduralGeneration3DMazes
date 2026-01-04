using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    public class GraphNode
    {
        public MazePoint Point { get; set; } = null!;
        public Direction[] ShortestPathDirections { get; set; } = [];
        public int ShortestPath { get; set; }
        public required GraphEdge[] Edges { get; set; }
    }
}

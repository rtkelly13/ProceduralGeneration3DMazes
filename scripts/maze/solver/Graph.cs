using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    public class Graph
    {
        public Dictionary<MazePoint, GraphNode> Nodes { get; set; } = new();
    }
}

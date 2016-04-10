using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Solver
{
    public class Graph
    {
        public Dictionary<MazePoint, GraphNode> Nodes { get; set; }
    }
}
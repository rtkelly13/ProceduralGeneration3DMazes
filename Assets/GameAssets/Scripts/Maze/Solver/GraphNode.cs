using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Solver
{
    public class GraphNode
    {
        public List<Direction> ShortestPathDirections { get; set; }
        public int ShortestPath { get; set; }
        public List<GraphEdge> Edges { get; set; } 
    }
}
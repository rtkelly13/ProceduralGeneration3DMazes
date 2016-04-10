using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Solver
{
    public class GraphEdge
    {
        public MazePoint Point { get; set; }
        public List<Direction> DirectionsToPoint { get; set; }
    }
}
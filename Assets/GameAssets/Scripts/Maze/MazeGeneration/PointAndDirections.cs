using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class PointAndDirections
    {
        public MazePoint Point { get; set; }
        public List<Direction> Directions { get; set; }
    }
}
using System.Collections.Generic;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public class AlgorithmRunResults
    {
        public required IMazeCarver Carver { get; set; }
        public required List<DirectionAndPoint> DirectionsCarvedIn { get; set; }
    }
}

using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    /// <summary>
    /// Interface for computing K shortest paths through a maze.
    /// </summary>
    public interface IKShortestPathsSolver
    {
        /// <summary>
        /// Computes up to K shortest paths from start to end in the maze.
        /// </summary>
        /// <param name="jumper">The maze to solve.</param>
        /// <param name="k">Maximum number of paths to compute.</param>
        /// <returns>List of paths, ordered by distance (shortest first).</returns>
        List<PathResult> GetKShortestPaths(IMazeJumper jumper, int k);

        /// <summary>
        /// Computes up to K shortest paths using an existing graph.
        /// </summary>
        /// <param name="graph">Pre-built graph of the maze.</param>
        /// <param name="startPoint">Starting point.</param>
        /// <param name="endPoint">Ending point.</param>
        /// <param name="k">Maximum number of paths to compute.</param>
        /// <returns>List of paths, ordered by distance (shortest first).</returns>
        List<PathResult> GetKShortestPaths(Graph graph, MazePoint startPoint, MazePoint endPoint, int k);
    }
}

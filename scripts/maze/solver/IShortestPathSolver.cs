using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    public interface IShortestPathSolver
    {
        /// <summary>
        /// Computes the shortest path for a maze.
        /// </summary>
        ShortestPathResult GetGraph(IMazeJumper jumper);
    }
}

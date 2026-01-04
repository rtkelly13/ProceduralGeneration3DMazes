using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    public interface IGraphBuilder
    {
        /// <summary>
        /// Builds a graph from a maze.
        /// </summary>
        Graph GetGraphFromMaze(IMazeJumper jumper);
    }
}

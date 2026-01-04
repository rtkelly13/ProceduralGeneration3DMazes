using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public interface IMazeFactory
    {
        /// <summary>
        /// Creates a maze carver for building new mazes.
        /// </summary>
        IMazeCarver GetMazeCarver(IModelBuilder modelBuilder);

        /// <summary>
        /// Creates a maze jumper from an existing read-only model.
        /// Used for imported mazes that don't need modification capabilities.
        /// The dead-end wrapping is initialized and dead-ends are computed.
        /// </summary>
        IMazeJumper GetMazeJumperFromModel(IModel model, bool fillDeadEnds = true);
    }
}

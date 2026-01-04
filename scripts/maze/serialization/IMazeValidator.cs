using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Serialization
{
    public interface IMazeValidator
    {
        /// <summary>
        /// Validates the consistency of a maze model.
        /// Checks for bidirectional consistency, boundary violations, etc.
        /// </summary>
        MazeValidationResult Validate(IModel model);
    }
}

using System.Collections.Generic;

namespace ProceduralMaze.Maze.Serialization
{
    /// <summary>
    /// Result of maze validation containing validity status and any errors found.
    /// </summary>
    public class MazeValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; }

        public MazeValidationResult()
        {
            Errors = new List<string>();
        }

        public MazeValidationResult(List<string> errors)
        {
            Errors = errors;
        }

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}

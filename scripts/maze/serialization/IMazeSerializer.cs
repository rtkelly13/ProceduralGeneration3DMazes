using System.IO;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Serialization
{
    public interface IMazeSerializer
    {
        /// <summary>
        /// Serializes a maze model to the output stream.
        /// </summary>
        void Serialize(IModel model, Stream output);

        /// <summary>
        /// Serializes a maze model to a string.
        /// </summary>
        string SerializeToString(IModel model);
    }
}

using System.IO;
using ProceduralMaze.Maze.Factory;

namespace ProceduralMaze.Maze.Serialization
{
    /// <summary>
    /// Serializes maze generation statistics to a text-based format.
    /// </summary>
    public interface IMazeStatsSerializer
    {
        /// <summary>
        /// Serializes maze generation results (statistics) to the output stream.
        /// </summary>
        void Serialize(MazeGenerationResults results, Stream output);

        /// <summary>
        /// Serializes maze generation results (statistics) to a string.
        /// </summary>
        string SerializeToString(MazeGenerationResults results);
    }
}

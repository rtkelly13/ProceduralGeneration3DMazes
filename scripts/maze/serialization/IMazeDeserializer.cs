using System;
using System.IO;

namespace ProceduralMaze.Maze.Serialization
{
    public interface IMazeDeserializer
    {
        /// <summary>
        /// Deserializes a maze from the input stream.
        /// </summary>
        ReadOnlyMazeModel Deserialize(Stream input);

        /// <summary>
        /// Deserializes a maze from a span of characters.
        /// </summary>
        ReadOnlyMazeModel Deserialize(ReadOnlySpan<char> content);

        /// <summary>
        /// Deserializes a maze from a string.
        /// </summary>
        ReadOnlyMazeModel DeserializeFromString(string content);
    }
}

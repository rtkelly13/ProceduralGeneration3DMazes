namespace ProceduralMaze.Maze.Factory
{
    /// <summary>
    /// Specifies the internal data structure used to store maze connections.
    /// </summary>
    public enum MazeType
    {
        None = 0,
        /// <summary>
        /// Array-based storage where each edge is stored only once (unidirectional).
        /// Uses less memory but requires neighbor lookups for some directions.
        /// </summary>
        ArrayUnidirectional = 1,
        /// <summary>
        /// Array-based storage where each edge is stored in both cells (bidirectional).
        /// Uses more memory but provides O(1) lookup for all directions.
        /// </summary>
        ArrayBidirectional = 2,
        /// <summary>
        /// Dictionary-based storage with bidirectional edges.
        /// Better for sparse mazes or when dynamic sizing is needed.
        /// </summary>
        Dictionary = 3
    }
}

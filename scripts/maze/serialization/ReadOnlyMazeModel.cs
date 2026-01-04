using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Serialization
{
    /// <summary>
    /// A read-only maze model backed by a dictionary.
    /// Used for deserialized mazes that don't need modification.
    /// </summary>
    public class ReadOnlyMazeModel : IModel
    {
        private readonly Dictionary<MazePoint, Direction> _cells;

        public MazeSize Size { get; }
        public MazePoint StartPoint { get; }
        public MazePoint EndPoint { get; }

        public ReadOnlyMazeModel(
            MazeSize size,
            MazePoint startPoint,
            MazePoint endPoint,
            Dictionary<MazePoint, Direction> cells)
        {
            Size = size;
            StartPoint = startPoint;
            EndPoint = endPoint;
            _cells = cells;
        }

        public Direction GetFlagFromPoint(MazePoint p)
        {
            return _cells.TryGetValue(p, out var directions) ? directions : Direction.None;
        }

        public bool HasDirections(MazePoint p, Direction d)
        {
            return (GetFlagFromPoint(p) & d) == d;
        }
    }
}

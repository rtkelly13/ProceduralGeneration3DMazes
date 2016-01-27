using System.Collections.Generic;
using MazeGeneration.Factory;
using MazeGeneration.Model;

namespace MazeGeneration
{
    public interface IMaze
    {
        Direction GetFlagFromPoint();
        IEnumerable<Direction> GetsDirectionsFromPoint();

        bool HasVertexes(Direction flag);
        void MoveInDirection(Direction d);

        MazePoint CurrentPoint { get; }
        MazeSize Size { get; }
    }
}

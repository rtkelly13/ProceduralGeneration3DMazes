using System.Collections.Generic;
using MazeGeneration.Factory;
using MazeGeneration.Model;

namespace MazeGeneration
{
    public interface IMaze
    {
        bool HasVertexes(MazePoint p, Direction flag);
        Direction GetFlagFromPoint(MazePoint p);
        IEnumerable<Direction> GetsDirectionsFromPoint(MazePoint p);
        void BaseInitialise(MazeSize size, bool allVertexes);

        MazeSize Size { get; }
    }
}

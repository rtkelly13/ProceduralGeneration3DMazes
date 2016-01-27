using MazeGeneration.Factory;
using MazeGeneration.Model;

namespace MazeGeneration
{
    public interface IBuilder
    {
        void PlaceVertex(MazePoint p, Direction d);
        void RemoveVertex(MazePoint p, Direction d);

        void BaseInitialise(MazeSize size, bool allVertexes);
    }
}

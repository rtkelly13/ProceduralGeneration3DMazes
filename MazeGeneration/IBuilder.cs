using MazeGeneration.Model;

namespace MazeGeneration
{
    public interface IBuilder: IMaze
    {
        void PlaceVertex(MazePoint p, Direction d);
        void RemoveVertex(MazePoint p, Direction d);
    }
}

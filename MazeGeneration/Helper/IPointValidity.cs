using MazeGeneration.Factory;
using MazeGeneration.Model;

namespace MazeGeneration.Helper
{
    public interface IPointValidity
    {
        bool ValidPoint(MazePoint p, MazeSize size);
    }
}
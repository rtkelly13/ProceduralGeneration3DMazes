using MazeGeneration.Factory;
using MazeGeneration.Model;

namespace MazeGeneration.MazeGeneration
{
    public interface IRandomPointGenerator
    {
        MazePoint RandomPoint(MazeSize size);
    }
}
using MazeGeneration.Model;

namespace MazeGeneration.Factory
{
    public interface IMazePointFactory
    {
        MazePoint MakePoint(int x, int y, int z);
    }
}
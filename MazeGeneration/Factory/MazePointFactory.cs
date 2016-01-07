using MazeGeneration.Model;

namespace MazeGeneration.Factory
{
    public class MazePointFactory : IMazePointFactory
    {
        public MazePoint MakePoint(int x, int y, int z)
        {
            return new MazePoint(x, y, z);
        }
    }
}

using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class MazePointFactory : IMazePointFactory
    {
        private readonly ObjectPool<MazePoint> pool; 
        public MazePointFactory()
        {
            pool = new ObjectPool<MazePoint>(5);
        }
        public MazePoint MakePoint(int x, int y, int z)
        {
            return pool.New().Set(x, y, z);
        }

        public void Store(MazePoint p)
        {
            pool.Store(p);
        }
    }
}

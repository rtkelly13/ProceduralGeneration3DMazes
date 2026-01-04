using ProceduralMaze.Maze.Factory;

namespace ProceduralMaze.Maze.Model
{
    public class MazeArrayBuilder : IMazeArrayBuilder
    {
        private readonly IMazePointFactory _pointFactory;

        public MazeArrayBuilder(IMazePointFactory pointFactory)
        {
            _pointFactory = pointFactory;
        }

        public MazeCell[,,] Build(MazeSize size)
        {
            var maze = new MazeCell[size.X, size.Y, size.Z];
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        maze[x, y, z] = new MazeCell
                        {
                            Directions = Direction.None
                        };
                    }
                }
            }
            return maze;
        }
    }
}

namespace ProceduralMaze.Maze.Model
{
    public class MazeSize
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public MazeSize()
        {
        }

        public MazeSize(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}

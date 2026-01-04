namespace ProceduralMaze.Maze.Model
{
    public class MazeCell
    {
        public MazeCell()
        {
            Directions = Direction.None;
        }

        public MazeCell(Direction directions)
        {
            Directions = directions;
        }

        public Direction Directions { get; set; }
    }
}

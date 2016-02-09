namespace Assets.GameAssets.Scripts.Maze.Model
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

namespace ProceduralMaze.Maze.Model
{
    public interface IMazeCarver : IMazeJumper
    {
        Direction[] CarvableDirections();
        Direction CarvableFlag();

        Direction[] AlreadyCarvedDirections();
        Direction AlreadyCarvedFlag();

        bool CanCarveInDirection(Direction d);
        void CarveInDirection(Direction d);
        void FillInDirection(Direction d);
        bool AlreadyCarvedDirection(Direction d);

        IMazeJumper CarvingFinished();
    }
}

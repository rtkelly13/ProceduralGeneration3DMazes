namespace ProceduralMaze.Maze.Model
{
    public interface IModel
    {
        bool HasDirections(MazePoint p, Direction d);
        Direction GetFlagFromPoint(MazePoint p);

        MazeSize Size { get; }
        MazePoint StartPoint { get; }
        MazePoint EndPoint { get; }
    }
}

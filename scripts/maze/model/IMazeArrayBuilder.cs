namespace ProceduralMaze.Maze.Model
{
    public interface IMazeArrayBuilder
    {
        MazeCell[,,] Build(MazeSize size);
    }
}

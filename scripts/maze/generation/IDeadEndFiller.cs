using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public interface IDeadEndFiller
    {
        DeadEndFillerResult Fill(IMazeCarver mazeCarver);
    }
}

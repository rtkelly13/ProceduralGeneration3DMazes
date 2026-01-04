using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public interface IMazeModelFactory
    {
        IModelBuilder BuildMaze(MazeGenerationSettings settings);
    }
}

using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public interface IModelsWrapperFactory
    {
        IModelsWrapper Make(IModelBuilder modelBuilder);
    }
}

using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public class ModelsWrapperFactory : IModelsWrapperFactory
    {
        public IModelsWrapper Make(IModelBuilder modelBuilder)
        {
            return new ModelsWrapper(modelBuilder);
        }
    }
}

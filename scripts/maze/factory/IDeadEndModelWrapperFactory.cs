using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public interface IDeadEndModelWrapperFactory
    {
        IDeadEndModelWrapper MakeModel(IModelBuilder model);
    }
}

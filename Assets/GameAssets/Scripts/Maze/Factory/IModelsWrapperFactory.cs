using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public interface IModelsWrapperFactory
    {
        IModelsWrapper Make(IModelBuilder modelBuilder);
    }
}
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public interface IDeadEndModelWrapperFactory
    {
        IDeadEndModelWrapper MakeModel(IModelBuilder model);
    }
}
using System;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public interface IModelsWrapper : IModelState
    {
        IModelBuilder ModelBuilder { get; }
        void DoDeadEndWrapping(Func<IModelBuilder, IDeadEndModelWrapper> modelAction);
    }

    public interface IModelState
    {
        bool DeadEnded { get; }
        ModelMode ModelMode { get; }
        void SetState(ModelMode mode);
    }
}
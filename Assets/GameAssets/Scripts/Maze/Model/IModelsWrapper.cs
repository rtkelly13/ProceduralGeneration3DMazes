using System;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public interface IModelsWrapper
    {
        bool DeadEnded { get; }
        ModelMode ModelMode { get; }
        IModelBuilder ModelBuilder { get; }

        void DoDeadEndWrapping(Func<IModelBuilder, IDeadEndModelWrapper> modelAction);
        void SetState(ModelMode mode);
    }
}
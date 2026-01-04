using System;

namespace ProceduralMaze.Maze.Model
{
    public class ModelsWrapper : IModelsWrapper
    {
        public ModelMode ModelMode { get; private set; }
        public IModelBuilder ModelBuilder { get; private set; } = null!;

        public ModelsWrapper(IModelBuilder mainModel)
        {
            _mainModel = mainModel;
            SetState(ModelMode.Standard);
        }

        private readonly IModelBuilder _mainModel;
        private IDeadEndModelWrapper? _deadEndModelWrapper;

        public void DoDeadEndWrapping(Func<IModelBuilder, IDeadEndModelWrapper> modelAction)
        {
            _deadEndModelWrapper = modelAction(_mainModel);
        }

        public bool DeadEnded
        {
            get { return ModelMode == ModelMode.DeadEndFilled; }
        }

        public void SetState(ModelMode mode)
        {
            switch (mode)
            {
                case ModelMode.Standard:
                    ModelMode = ModelMode.Standard;
                    ModelBuilder = _mainModel;
                    break;
                case ModelMode.DeadEndFilled:
                    if (_deadEndModelWrapper != null)
                    {
                        ModelMode = ModelMode.DeadEndFilled;
                        ModelBuilder = _deadEndModelWrapper;
                    }
                    else
                    {
                        throw new ArgumentException("Dead end Mode has not been initialised");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum ModelMode
    {
        Standard = 0,
        DeadEndFilled = 1
    }
}

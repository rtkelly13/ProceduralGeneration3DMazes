using System;

namespace ProceduralMaze.Maze.Model
{
    /// <summary>
    /// Wraps a read-only IModel to satisfy the IModelsWrapper interface.
    /// Used for imported mazes that don't need modification capabilities.
    /// </summary>
    public class ReadOnlyModelsWrapper : IModelsWrapper
    {
        private readonly IModel _model;
        private readonly ReadOnlyModelBuilderAdapter _adapter;
        private IDeadEndModelWrapper? _deadEndModelWrapper;

        public ModelMode ModelMode { get; private set; }
        public IModelBuilder ModelBuilder { get; private set; }

        public bool DeadEnded => ModelMode == ModelMode.DeadEndFilled;

        public ReadOnlyModelsWrapper(IModel model)
        {
            _model = model;
            _adapter = new ReadOnlyModelBuilderAdapter(model);
            ModelBuilder = _adapter;
            ModelMode = ModelMode.Standard;
        }

        public void SetState(ModelMode mode)
        {
            switch (mode)
            {
                case ModelMode.Standard:
                    ModelMode = ModelMode.Standard;
                    ModelBuilder = _adapter;
                    break;
                case ModelMode.DeadEndFilled:
                    if (_deadEndModelWrapper != null)
                    {
                        ModelMode = ModelMode.DeadEndFilled;
                        ModelBuilder = _deadEndModelWrapper;
                    }
                    else
                    {
                        // Dead-end wrapping hasn't been computed yet
                        // For imported mazes, we can still compute it on demand
                        throw new InvalidOperationException(
                            "Dead end mode has not been initialized. Call DoDeadEndWrapping first.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        public void DoDeadEndWrapping(Func<IModelBuilder, IDeadEndModelWrapper> modelAction)
        {
            // The adapter implements IModelBuilder, so we can pass it to the dead-end wrapper
            _deadEndModelWrapper = modelAction(_adapter);
        }

        /// <summary>
        /// Internal adapter that wraps IModel and implements IModelBuilder with no-op write operations.
        /// This is hidden from external consumers - they only see the clean IModel interface.
        /// </summary>
        private sealed class ReadOnlyModelBuilderAdapter : IModelBuilder
        {
            private readonly IModel _model;

            public ReadOnlyModelBuilderAdapter(IModel model)
            {
                _model = model;
            }

            public MazeSize Size => _model.Size;
            public MazePoint StartPoint => _model.StartPoint;
            public MazePoint EndPoint => _model.EndPoint;

            public Direction GetFlagFromPoint(MazePoint p) => _model.GetFlagFromPoint(p);
            public bool HasDirections(MazePoint p, Direction d) => _model.HasDirections(p, d);

            // No-op write operations - imported mazes are read-only
            public void PlaceVertex(MazePoint p, Direction d)
            {
                // No-op: imported mazes cannot be modified
            }

            public void RemoveVertex(MazePoint p, Direction d)
            {
                // No-op: imported mazes cannot be modified
            }
        }
    }
}

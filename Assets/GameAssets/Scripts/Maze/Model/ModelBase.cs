using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public abstract class ModelBase: IModelBuilder
    {
        public abstract bool HasDirections(MazePoint p, Direction d);

        public abstract Direction GetFlagFromPoint(MazePoint p);

        public abstract void PlaceVertex(MazePoint p, Direction d);
        public abstract void RemoveVertex(MazePoint p, Direction d);

        public ModelBase BaseInitialise(ModelInitialisationOptions options)
        {
            Size = options.Size;
            StartPoint = options.StartPoint;
            EndPoint = options.EndPoint;
            Initialise(options);
            return this;
        }

        protected abstract void Initialise(ModelInitialisationOptions options);

        public MazeSize Size { get; private set; }
        public MazePoint StartPoint { get; private set; }
        public MazePoint EndPoint { get; private set; }
    }
}
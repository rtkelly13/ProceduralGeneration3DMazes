namespace MazeGeneration.Factory
{
    public interface IModelFactory
    {
        IBuilder MakeModel(MazeType type, MazeSize size);
    }
}
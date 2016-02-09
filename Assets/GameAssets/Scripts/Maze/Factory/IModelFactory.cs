namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public interface IModelFactory
    {
        IBuilder MakeModel(MazeType type, MazeSize size);
    }
}
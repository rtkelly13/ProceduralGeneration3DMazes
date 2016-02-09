namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public interface IRandomValueGenerator
    {
        int GetNext(int min, int max);
    }
}
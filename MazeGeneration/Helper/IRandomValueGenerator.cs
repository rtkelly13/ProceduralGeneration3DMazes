namespace MazeGeneration.Helper
{
    public interface IRandomValueGenerator
    {
        int GetNext(int min, int max);
    }
}
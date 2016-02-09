using System;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public interface IMazeGenerationFactory
    {
        IMaze GenerateMaze(MazeGenerationOptions options);
    }

    public class MazeGenerationFactory : IMazeGenerationFactory
    {
        public IMaze GenerateMaze(MazeGenerationOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

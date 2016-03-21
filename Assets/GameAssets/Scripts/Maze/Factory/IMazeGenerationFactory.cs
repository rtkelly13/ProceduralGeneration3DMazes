using System;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public interface IMazeGenerationFactory
    {
        IMaze GenerateMaze(MazeGenerationSettings options);
    }

    public class MazeGenerationFactory : IMazeGenerationFactory
    {
        public MazeGenerationFactory()
        {
            
        }
        public IMaze GenerateMaze(MazeGenerationSettings options)
        {
            switch (options.Algorithm)
            {
                case Algorithm.None:
                    throw new ArgumentException("None not supported");
                case Algorithm.GrowingTreeAlgorithm:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unsupported algorithm type");
            }
        }
    }
}

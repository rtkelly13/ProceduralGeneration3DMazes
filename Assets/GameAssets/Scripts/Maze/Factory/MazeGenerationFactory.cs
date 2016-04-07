using System;
using Assets.Examples.Breakout;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI;

namespace Assets.GameAssets.Scripts.Maze.Factory
{
    public class MazeGenerationFactory : IMazeGenerationFactory
    {
        private readonly IMazeModelFactory _mazeModelFactory;
        private readonly IGrowingTreeAlgorithm _growingTreeAlgorithm;
        private readonly IMazeFactory _mazeFactory;
        private readonly IDeadEndFiller _deadEndFiller;
        private readonly IRandomCarver _randomCarver;
        private readonly IExtraWallCalculator _extraWall;
        private readonly IRecursiveBacktrackerAlgorithm _recursiveBacktrackerAlgorithm;
        private readonly IBinaryTreeAlgorithm _binaryTreeAlgorithm;

        public MazeGenerationFactory(IMazeModelFactory mazeModelFactory, IGrowingTreeAlgorithm growingTreeAlgorithm, IMazeFactory mazeFactory, IDeadEndFiller deadEndFiller, IRandomCarver randomCarver, IExtraWallCalculator extraWall, IRecursiveBacktrackerAlgorithm recursiveBacktrackerAlgorithm, IBinaryTreeAlgorithm binaryTreeAlgorithm)
        {
            _mazeModelFactory = mazeModelFactory;
            _growingTreeAlgorithm = growingTreeAlgorithm;
            _mazeFactory = mazeFactory;
            _deadEndFiller = deadEndFiller;
            _randomCarver = randomCarver;
            _extraWall = extraWall;
            _recursiveBacktrackerAlgorithm = recursiveBacktrackerAlgorithm;
            _binaryTreeAlgorithm = binaryTreeAlgorithm;
        }

        public IMazeJumper GenerateMaze(MazeGenerationSettings settings)
        {
            var model = _mazeModelFactory.BuildMaze(settings.Option, settings.Size);
            var carver = _mazeFactory.GetMazeCarver(model);
            
            switch (settings.Algorithm)
            {
                case Algorithm.None:
                    throw new ArgumentException("None not supported");
                case Algorithm.GrowingTreeAlgorithm:
                    carver = _growingTreeAlgorithm.GenerateMaze(carver, settings);
                    break;
                case Algorithm.RecursiveBacktrackerAlgorithm:
                    carver = _recursiveBacktrackerAlgorithm.GenerateMaze(carver, settings);  
                    break;
                case Algorithm.BinaryTreeAlgorithm:
                    carver = _binaryTreeAlgorithm.GenerateMaze(carver, settings);
                    break;
                default:
                    throw new ArgumentException("Unsupported algorithm type");
            }
            _randomCarver.CarveRandomWalls(carver, settings.ExtraWalls, _extraWall.Calulate(carver.Size));
            _deadEndFiller.Fill(carver);
            return carver.CarvingFinished();
        }
    }
}
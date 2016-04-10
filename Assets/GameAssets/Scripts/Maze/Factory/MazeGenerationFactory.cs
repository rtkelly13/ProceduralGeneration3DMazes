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
        private readonly IHeuristicsGenerator _heuristicsGenerator;

        public MazeGenerationFactory(IMazeModelFactory mazeModelFactory, IGrowingTreeAlgorithm growingTreeAlgorithm, IMazeFactory mazeFactory, IDeadEndFiller deadEndFiller, IRandomCarver randomCarver, IExtraWallCalculator extraWall, IRecursiveBacktrackerAlgorithm recursiveBacktrackerAlgorithm, IBinaryTreeAlgorithm binaryTreeAlgorithm, IHeuristicsGenerator heuristicsGenerator)
        {
            _mazeModelFactory = mazeModelFactory;
            _growingTreeAlgorithm = growingTreeAlgorithm;
            _mazeFactory = mazeFactory;
            _deadEndFiller = deadEndFiller;
            _randomCarver = randomCarver;
            _extraWall = extraWall;
            _recursiveBacktrackerAlgorithm = recursiveBacktrackerAlgorithm;
            _binaryTreeAlgorithm = binaryTreeAlgorithm;
            _heuristicsGenerator = heuristicsGenerator;
        }

        public MazeGenerationResults GenerateMaze(MazeGenerationSettings settings)
        {
            var model = _mazeModelFactory.BuildMaze(settings);
            var carver = _mazeFactory.GetMazeCarver(model);
            AlgorithmRunResults results = null;
            switch (settings.Algorithm)
            {
                case Algorithm.None:
                    throw new ArgumentException("None not supported");
                case Algorithm.GrowingTreeAlgorithm:
                    results = _growingTreeAlgorithm.GenerateMaze(carver, settings);
                    break;
                case Algorithm.RecursiveBacktrackerAlgorithm:
                    results = _recursiveBacktrackerAlgorithm.GenerateMaze(carver, settings);
                    break;
                case Algorithm.BinaryTreeAlgorithm:
                    results = _binaryTreeAlgorithm.GenerateMaze(carver, settings);
                    break;
                default:
                    throw new ArgumentException("Unsupported algorithm type");
            }
            carver = results.Carver;
            _randomCarver.CarveRandomWalls(carver, settings.ExtraWalls, _extraWall.Calulate(carver.Size));
            var deadEndFillerResults = _deadEndFiller.Fill(carver);
            return new MazeGenerationResults
            {
                MazeJumper = carver.CarvingFinished(),
                HeuristicsResults = _heuristicsGenerator.GetResults(carver),
                DeadEndFillerResults = deadEndFillerResults
            };
        }
    }
}
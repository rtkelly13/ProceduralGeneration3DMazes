using System;
using System.Diagnostics;
using System.Linq;
using Assets.Examples.Breakout;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Heuristics;
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
        private readonly IAgentFactory _agentFactory;
        private readonly ITimeRecorder _timeRecorder;

        public MazeGenerationFactory(
            IMazeModelFactory mazeModelFactory,
            IGrowingTreeAlgorithm growingTreeAlgorithm,
            IMazeFactory mazeFactory, 
            IDeadEndFiller deadEndFiller, 
            IRandomCarver randomCarver, 
            IExtraWallCalculator extraWall, 
            IRecursiveBacktrackerAlgorithm recursiveBacktrackerAlgorithm, 
            IBinaryTreeAlgorithm binaryTreeAlgorithm, 
            IHeuristicsGenerator heuristicsGenerator, 
            IAgentFactory agentFactory,
            ITimeRecorder timeRecorder)
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
            _agentFactory = agentFactory;
            _timeRecorder = timeRecorder;
        }

        public MazeGenerationResults GenerateMaze(MazeGenerationSettings settings)
        {
            IMazeCarver carver = null;
            var modelBuildTime = _timeRecorder.GetRunningTime(() =>
            {
                var model = _mazeModelFactory.BuildMaze(settings);
                carver = _mazeFactory.GetMazeCarver(model);
            });
            
            AlgorithmRunResults results = null;
            int extraWallsAdded = 0;
            var generationTime = _timeRecorder.GetRunningTime(() =>
            {
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
                extraWallsAdded = _extraWall.Calulate(carver.Size);
                _randomCarver.CarveRandomWalls(carver, settings.ExtraWalls,extraWallsAdded);
            });

            DeadEndFillerResult deadEndFillerResults = null;
            var deadEndFillerTime = _timeRecorder.GetRunningTime(() =>
            {
                deadEndFillerResults = _deadEndFiller.Fill(carver);
            });
            AgentResults result = null;
            var agentGenerationTime = _timeRecorder.GetRunningTime(() =>
            {
                if (settings.AgentType != AgentType.None)
                {
                    result = _agentFactory.MakeAgent(settings.AgentType).RunAgent(carver);
                }
            });

            HeuristicsResults heuristicsResults = null;
            var heuristicsTime = _timeRecorder.GetRunningTime(() =>
            {
                heuristicsResults = _heuristicsGenerator.GetResults(carver);
            });

            var times = new []{ modelBuildTime, generationTime, deadEndFillerTime, agentGenerationTime, heuristicsTime };
            var totalTime = times.Aggregate(new TimeSpan(), (seed, value) => seed.Add(value));

            return new MazeGenerationResults
            {
                MazeJumper = carver.CarvingFinished(),
                HeuristicsResults = heuristicsResults,
                DeadEndFillerResults = deadEndFillerResults,
                ModelTime = modelBuildTime,
                AgentResults = result,
                GenerationTime = generationTime,
                DeadEndFillerTime = deadEndFillerTime,
                AgentGenerationTime = agentGenerationTime,
                HeuristicsTime = heuristicsTime,
                TotalTime = totalTime
            };
        }
    }
}
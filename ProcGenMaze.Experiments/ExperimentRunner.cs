

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI;
using Assets.GameAssets.Scripts.UI.Helper;
using MoreLinq;

namespace ProcGenMaze.Experiments
{
    public class ExperimentRunner : IExperimentRunner
    {
        public MazeGenerationSettings Settings1 { get; set; }
        public MazeGenerationSettings Settings2 { get; set; }
        public MazeGenerationSettings Settings3 { get; set; }
        public MazeGenerationSettings Settings4 { get; set; }
        public MazeGenerationSettings Settings5 { get; set; }
        public MazeGenerationSettings Settings6 { get; set; }
        public MazeGenerationSettings Settings7 { get; set; }
        public MazeGenerationSettings Settings8 { get; set; }
        public MazeGenerationSettings Settings9 { get; set; }
        public MazeGenerationSettings Settings10 { get; set; }
        public MazeGenerationSettings Settings11 { get; set; }
        public MazeGenerationSettings Settings12 { get; set; }

        private readonly IMazeGenerationFactory _mazeGenerationFactory;
        private readonly IArrayHelper _arrayHelper;
        private readonly IOutputWriter _outputWriter;
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public ExperimentRunner(IMazeGenerationFactory mazeGenerationFactory, IArrayHelper arrayHelper, IOutputWriter outputWriter, IDirectionsFlagParser directionsFlagParser)
        {
            _mazeGenerationFactory = mazeGenerationFactory;
            _arrayHelper = arrayHelper;
            _outputWriter = outputWriter;
            _directionsFlagParser = directionsFlagParser;

            Settings1 = new MazeGenerationSettings
            {
                Algorithm = Algorithm.BinaryTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = true,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 }
            };

            Settings2 = new MazeGenerationSettings
            {
                Algorithm = Algorithm.BinaryTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = false,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 }
            };

            Settings3 = new MazeGenerationSettings
            {
                Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = true,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 }
            };

            Settings4 = new MazeGenerationSettings
            {
                Algorithm = Algorithm.RecursiveBacktrackerAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = false,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 }
            };

            Settings5 = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = true,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 },
                Strategies = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Newest}
            };

            Settings6 = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = false,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 },
                Strategies = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Newest }
            };

            Settings7 = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = true,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 },
                Strategies = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Random }
            };

            Settings8 = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = false,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 },
                Strategies = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Random }
            };

            Settings9 = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = true,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 },
                Strategies = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Newest, GrowingTreeStrategy.Newest, GrowingTreeStrategy.Newest, GrowingTreeStrategy.RandomOldest, GrowingTreeStrategy.Middle }
            };

            Settings10 = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = false,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 },
                Strategies = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Newest, GrowingTreeStrategy.Newest, GrowingTreeStrategy.Newest, GrowingTreeStrategy.RandomOldest, GrowingTreeStrategy.Middle }
            };

            Settings11 = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = true,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 },
                Strategies = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Newest, GrowingTreeStrategy.Oldest}
            };

            Settings12 = new GrowingTreeSettings
            {
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                AgentType = AgentType.None,
                DoorsAtEdge = false,
                ExtraWalls = WallCarverOption.Random,
                Option = MazeType.DirectedMaze,
                Size = new MazeSize { X = 25, Y = 25, Z = 25 },
                Strategies = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Newest, GrowingTreeStrategy.Oldest }
            };



        }

        public void Run()
        {  
            _outputWriter.PrintLn("Binary tree force edge");
            RunForSettings(Settings1, true, 500);
            _outputWriter.PrintLn("Binary tree ");
            RunForSettings(Settings2, true, 500);
            _outputWriter.PrintLn("Recursive backtracker force edge");
            RunForSettings(Settings3, true, 500);
            _outputWriter.PrintLn("Recursive backtracker");
            RunForSettings(Settings4, true, 500);
            _outputWriter.PrintLn("Growing Tree Recursive Mimicked force edge ");
            RunForSettings(Settings5, true);
            _outputWriter.PrintLn("Growing tree Recursive Mimicked");
            RunForSettings(Settings6, true);
            _outputWriter.PrintLn("Growing Tree Prims Mimicked force edge ");
            RunForSettings(Settings7, true);
            _outputWriter.PrintLn("Growing tree Prims Mimicked");
            RunForSettings(Settings8, true);
            _outputWriter.PrintLn("Growing Tree 3New, 1RandOld, 1Middle forced edge ");
            RunForSettings(Settings9, true);
            _outputWriter.PrintLn("Growing tree 3New, 1RandOld, 1Middle");
            RunForSettings(Settings10, true);
            _outputWriter.PrintLn("Growing Tree 1New, 1Old forced edge ");
            RunForSettings(Settings9, true);
            _outputWriter.PrintLn("Growing tree 1New, 1Old");
            RunForSettings(Settings10, true);

            Console.ReadLine();
        }

        public void RunForSettings(MazeGenerationSettings settings, bool parallel = false, int iterations = 100)
        {
            _outputWriter.Print("Random ");
            Settings2.ExtraWalls = WallCarverOption.Random;
            var results1 = RunExperiment(settings, iterations, parallel);
            _outputWriter.PrintLn("Finshed");

            _outputWriter.Print("Dead end ");
            Settings2.ExtraWalls = WallCarverOption.DeadEndWithPreferredDirection;
            var results2 = RunExperiment(settings, iterations, parallel);
            _outputWriter.PrintLn("Finshed");

            _outputWriter.Print("Dead end preferred ");
            Settings2.ExtraWalls = WallCarverOption.DeadEnd;
            var results3 = RunExperiment(settings, iterations, parallel);
            _outputWriter.PrintLn("Finshed");

            _outputWriter.Print("None ");
            Settings2.ExtraWalls = WallCarverOption.None;
            var results4 = RunExperiment(settings, iterations, parallel);
            _outputWriter.PrintLn("Finshed");

            _outputWriter.PrintLn($"Random");
            PrintResults(results1);
            _outputWriter.PrintLn($"Dead end");
            PrintResults(results2);
            _outputWriter.PrintLn($"Dead end preferred");
            PrintResults(results3);
            _outputWriter.PrintLn($"None");
            PrintResults(results4);
        }

        private void PrintResults(ExperimentResult result)
        {
            _outputWriter.Print($"Average shortest path - { result.AverageShortestPath } ");
            _outputWriter.Print($"Average generation time - { result.AverageGenerationTime } ");
            _outputWriter.Print("Average direction weightings: ");
            var strings = result.DirectionWeightings.Select(x => $"{x.Key} - {x.Value}");
            _outputWriter.Print(string.Join(", ", strings) + " ");
            _outputWriter.PrintLn($"Average cells filled in - { result.AverageCellsFilledIn }");
        }

        private IEnumerable<MazeResults> GenerateMazes(MazeGenerationSettings settings, int iterations)
        {
            var itemsToProcess = iterations;
            return Enumerable.Range(0, itemsToProcess).Select(x => BuildMaze(settings));
        }

        private ParallelQuery<MazeResults> GenerateMazesParallel(MazeGenerationSettings settings, int iterations)
        {
            var itemsToProcess = iterations;
            return Enumerable.Range(0, itemsToProcess).AsParallel().WithDegreeOfParallelism(4).Select(x => BuildMaze(settings));
        }

        private MazeResults BuildMaze(MazeGenerationSettings settings)
        {
            var result = _mazeGenerationFactory.GenerateMaze(settings);
            return new MazeResults
            {
                ShortestPath = result.HeuristicsResults.ShortestPathResult.ShortestPath,
                Stats = result.HeuristicsResults.Stats,
                ModelTime = result.ModelTime,
                GenerationTime = result.GenerationTime,
                DeadEndFillerTime = result.DeadEndFillerTime,
                AgentGenerationTime = result.AgentGenerationTime,
                HeuristicsTime = result.HeuristicsTime,
                TotalTime = result.TotalTime,
                TotalCellsFilledIn = result.DeadEndFillerResults.TotalCellsFilledIn
            };
        }

        private ExperimentResult RunExperiment(MazeGenerationSettings settings, int iterations, bool parallel)
        {
            var batch = iterations / 100;
            int count = 0;
            List<MazeResults> items = null;
            if (parallel)
            {
                items = GenerateMazesParallel(settings, iterations).Select(x =>
                {
                    count++;
                    if (count%batch == 0)
                    {
                        _outputWriter.Print($"{count/batch}% ");
                    }

                    return x;
                }).ToList();
            }
            else
            {
                items = GenerateMazes(settings, iterations).Select(x =>
                {
                    count++;
                    if (count % batch == 0)
                    {
                        _outputWriter.Print($"{count / batch}% ");
                    }

                    return x;
                }).ToList();
            }
            var doubleAverageTicks = _arrayHelper.Average(items, x => x.TotalTime.Ticks);
            long longAverageTicks = Convert.ToInt64(doubleAverageTicks);
            var result = new ExperimentResult
            {
                AverageShortestPath = _arrayHelper.Average(items, x => x.ShortestPath),
                AverageGenerationTime = new TimeSpan(longAverageTicks),
                DirectionWeightings = _directionsFlagParser.Directions.ToDictionary(x => x, y => _arrayHelper.Average(items, z => z.Stats.DirectionsUsed[y])),
                AverageCellsFilledIn = _arrayHelper.Average(items, x => x.TotalCellsFilledIn)
            };
            return result;
        }
    }
}

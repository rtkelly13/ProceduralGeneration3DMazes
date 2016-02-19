using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class GrowingTreeAlgorithm
    {
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IRandomValueGenerator _randomValueGenerator;
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public GrowingTreeAlgorithm(IRandomPointGenerator randomPointGenerator, IRandomValueGenerator randomValueGenerator, IDirectionsFlagParser directionsFlagParser)
        {
            _randomPointGenerator = randomPointGenerator;
            _randomValueGenerator = randomValueGenerator;
            _directionsFlagParser = directionsFlagParser;
        }

        public void GenerateMaze(List<GrowingTreeStrategy> strategies, IMazeCarver initialisedMaze)
        {
            var randomPoint = _randomPointGenerator.RandomPoint(initialisedMaze.Size);
            var activeCells = new List<MazePoint> { randomPoint };
            while (!activeCells.Any())
            {
                var currentPoint = GetNextPoint(activeCells, strategies);
                initialisedMaze.JumpToPoint(currentPoint);
                var carvableDirections = initialisedMaze.CarvableDirections().Randomise();
                foreach (var direction in carvableDirections)
                {
                    if (initialisedMaze.TryJumpInDirection(direction))
                    {
                        var k = initialisedMaze.AlreadyCarvedDirections();
                        if (!k.Any())
                        {
                            var oppositeDirection = _directionsFlagParser.OppositeDirection(direction);
                            initialisedMaze.CarveInDirection(oppositeDirection);
                            activeCells.Add(initialisedMaze.CurrentPoint);
                            break;
                        }
                    }
                    initialisedMaze.JumpToPoint(currentPoint);
                }
                activeCells.Remove(currentPoint);
            }
        }

        public MazePoint GetNextPoint(List<MazePoint> activeCells, List<GrowingTreeStrategy> strategies)
        {
            var item = _randomValueGenerator.GetNext(0, strategies.Count - 1);
            var strategy = strategies[item];
            var middle = activeCells.Count / 2;
            switch (strategy)
            {
                case GrowingTreeStrategy.First:
                    return activeCells.First();
                case GrowingTreeStrategy.Last:
                    return activeCells.Last();
                case GrowingTreeStrategy.Middle:
                    return activeCells[middle];
                case GrowingTreeStrategy.Random:
                    var randomIndex = _randomValueGenerator.GetNext(0, activeCells.Count - 1);
                    return activeCells[randomIndex];
                case GrowingTreeStrategy.RandomFirst:
                    var randomFirstIndex = _randomValueGenerator.GetNext(middle, activeCells.Count - 1);
                    return activeCells[randomFirstIndex];
                case GrowingTreeStrategy.RandomLast:
                    var randomLastIndex = _randomValueGenerator.GetNext(0, middle);
                    return activeCells[randomLastIndex];
                default:
                    throw new ArgumentException("Unsupported Cell Selection Strategy");
            }
        }
    }
}

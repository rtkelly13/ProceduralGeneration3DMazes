using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class GrowingTreeAlgorithm : IGrowingTreeAlgorithm
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

        public IMazeCarver GenerateMaze(IMazeCarver initialisedMaze, MazeGenerationSettings settings)
        {
            var growingTreeSettings = settings as GrowingTreeSettings;
            if (growingTreeSettings == null)
            {
                throw new ArgumentException("The correct settings are not present");
            }
            return GenerateMaze(initialisedMaze, growingTreeSettings.Strategies);
        }

        private IMazeCarver GenerateMaze(IMazeCarver initialisedMaze, List<GrowingTreeStrategy> strategies)
        {
            var randomPoint = _randomPointGenerator.RandomPoint(initialisedMaze.Size);
            var activeCells = new List<MazePoint> { randomPoint };
            while (activeCells.Any())
            {
                var currentPoint = GetNextPoint(activeCells, strategies);
                initialisedMaze.JumpToPoint(currentPoint);
                var carvableDirections = initialisedMaze.CarvableDirections().ToList();
                carvableDirections.Shuffle();
                var carved = false;
                foreach (var direction in carvableDirections)
                {
                    initialisedMaze.JumpInDirection(direction);
                    var carvedDirections = initialisedMaze.AlreadyCarvedDirections();
                    if (!carvedDirections.Any())
                    {
                        var oppositeDirection = _directionsFlagParser.OppositeDirection(direction);
                        initialisedMaze.CarveInDirection(oppositeDirection);
                        activeCells.Add(initialisedMaze.CurrentPoint);
                        carved = true;
                        break;
                    }
                    initialisedMaze.JumpToPoint(currentPoint);
                }
                if (!carved)
                {
                    activeCells.Remove(currentPoint);
                }
            }
            return initialisedMaze;
        }

        private MazePoint GetNextPoint(List<MazePoint> activeCells, List<GrowingTreeStrategy> strategies)
        {
            var item = _randomValueGenerator.GetNext(0, strategies.Count - 1);
            var strategy = strategies[item];
            var middle = activeCells.Count / 2;
            switch (strategy)
            {
                case GrowingTreeStrategy.Oldest:
                    return activeCells.First();
                case GrowingTreeStrategy.Newest:
                    return activeCells.Last();
                case GrowingTreeStrategy.Middle:
                    return activeCells[middle];
                case GrowingTreeStrategy.Random:
                    var randomIndex = _randomValueGenerator.GetNext(0, activeCells.Count - 1);
                    return activeCells[randomIndex];
                case GrowingTreeStrategy.RandomOldest:
                    var randomFirstIndex = _randomValueGenerator.GetNext(middle, activeCells.Count - 1);
                    return activeCells[randomFirstIndex];
                case GrowingTreeStrategy.RandomNewest:
                    var randomLastIndex = _randomValueGenerator.GetNext(0, middle);
                    return activeCells[randomLastIndex];
                default:
                    throw new ArgumentException("Unsupported Cell Selection Strategy");
            }
        }
    }
}

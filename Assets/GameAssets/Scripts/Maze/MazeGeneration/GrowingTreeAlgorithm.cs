using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class GrowingTreeAlgorithm : IGrowingTreeAlgorithm
    {
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IRandomValueGenerator _randomValueGenerator;
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IArrayHelper _arrayHelper;

        public GrowingTreeAlgorithm(IRandomPointGenerator randomPointGenerator, IRandomValueGenerator randomValueGenerator, IDirectionsFlagParser directionsFlagParser, IArrayHelper arrayHelper)
        {
            _randomPointGenerator = randomPointGenerator;
            _randomValueGenerator = randomValueGenerator;
            _directionsFlagParser = directionsFlagParser;
            _arrayHelper = arrayHelper;
        }

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            var growingTreeSettings = settings as GrowingTreeSettings;
            if (growingTreeSettings == null)
            {
                throw new ArgumentException("The correct settings are not present");
            }
            return GenerateMaze(maze, growingTreeSettings.Strategies);
        }

        private AlgorithmRunResults GenerateMaze(IMazeCarver maze, List<GrowingTreeStrategy> strategies)
        {
            var pointsAndDirections = new List<DirectionAndPoint>();
            var randomPoint = _randomPointGenerator.RandomPoint(maze.Size);
            var activeCells = new List<MazePoint> { randomPoint };
            while (activeCells.Any())
            {
                var currentPoint = GetNextPoint(activeCells, strategies);
                maze.JumpToPoint(currentPoint);
                var carvableDirections = maze.CarvableDirections().ToList();
                _arrayHelper.Shuffle(carvableDirections);
                var carved = false;
                foreach (var direction in carvableDirections)
                {
                    maze.JumpInDirection(direction);
                    var carvedDirections = maze.AlreadyCarvedDirections();
                    if (!carvedDirections.Any())
                    {
                        pointsAndDirections.Add(new DirectionAndPoint { Direction = direction, MazePoint = currentPoint });
                        var oppositeDirection = _directionsFlagParser.OppositeDirection(direction);
                        maze.CarveInDirection(oppositeDirection);
                        activeCells.Add(maze.CurrentPoint);
                        carved = true;
                        break;
                    }
                    maze.JumpToPoint(currentPoint);
                }
                if (!carved)
                {
                    activeCells.Remove(currentPoint);
                }
            }
            return new AlgorithmRunResults
            {
                Carver = maze,
                DirectionsCarvedIn = pointsAndDirections
            };
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

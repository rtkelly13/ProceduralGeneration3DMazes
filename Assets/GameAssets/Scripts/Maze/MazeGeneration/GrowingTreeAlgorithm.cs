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

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            var growingTreeSettings = settings as GrowingTreeSettings;
            if (growingTreeSettings == null)
            {
                throw new ArgumentException("The correct settings are not present");
            }
            var carver = GenerateMaze(maze, growingTreeSettings.Strategies);
            return new AlgorithmRunResults
            {
                Carver = carver
            };
        }

        private IMazeCarver GenerateMaze(IMazeCarver maze, List<GrowingTreeStrategy> strategies)
        {
            var randomPoint = _randomPointGenerator.RandomPoint(maze.Size);
            var activeCells = new List<MazePoint> { randomPoint };
            while (activeCells.Any())
            {
                var currentPoint = GetNextPoint(activeCells, strategies);
                maze.JumpToPoint(currentPoint);
                var carvableDirections = maze.CarvableDirections().ToList();
                carvableDirections.Shuffle();
                var carved = false;
                foreach (var direction in carvableDirections)
                {
                    maze.JumpInDirection(direction);
                    var carvedDirections = maze.AlreadyCarvedDirections();
                    if (!carvedDirections.Any())
                    {
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
            return maze;
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

    public class GrowingTreeAlgorithmLinkedList : IGrowingTreeAlgorithm
    {
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IRandomValueGenerator _randomValueGenerator;
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public GrowingTreeAlgorithmLinkedList(IRandomPointGenerator randomPointGenerator, IRandomValueGenerator randomValueGenerator, IDirectionsFlagParser directionsFlagParser)
        {
            _randomPointGenerator = randomPointGenerator;
            _randomValueGenerator = randomValueGenerator;
            _directionsFlagParser = directionsFlagParser;
        }

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            var growingTreeSettings = settings as GrowingTreeSettings;
            if (growingTreeSettings == null)
            {
                throw new ArgumentException("The correct settings are not present");
            }
            var carver = GenerateMaze(maze, growingTreeSettings.Strategies);
            return new AlgorithmRunResults
            {
                Carver = carver
            };
        }

        private IMazeCarver GenerateMaze(IMazeCarver maze, List<GrowingTreeStrategy> strategies)
        {
            var randomPoint = _randomPointGenerator.RandomPoint(maze.Size);
            var activeCells = new LinkedList<MazePoint>();
            activeCells.AddFirst(randomPoint) ;
            while (activeCells.Any())
            {
                var currentPoint = GetNextPoint(activeCells, strategies);
                maze.JumpToPoint(currentPoint);
                var carvableDirections = maze.CarvableDirections().ToList();
                carvableDirections.Shuffle();
                var carved = false;
                foreach (var direction in carvableDirections)
                {
                    maze.JumpInDirection(direction);
                    var carvedDirections = maze.AlreadyCarvedDirections();
                    if (!carvedDirections.Any())
                    {
                        var oppositeDirection = _directionsFlagParser.OppositeDirection(direction);
                        maze.CarveInDirection(oppositeDirection);
                        activeCells.AddLast(maze.CurrentPoint);
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
            return maze;
        }

        private MazePoint GetNextPoint(LinkedList<MazePoint> activeCells, List<GrowingTreeStrategy> strategies)
        {
            var item = _randomValueGenerator.GetNext(0, strategies.Count - 1);
            var strategy = strategies[item];
            var middle = activeCells.Count / 2;
            switch (strategy)
            {
                case GrowingTreeStrategy.Oldest:
                    return activeCells.First.Value;
                case GrowingTreeStrategy.Newest:
                    return activeCells.Last.Value;
                case GrowingTreeStrategy.Middle:
                    return activeCells.ElementAt(middle);
                case GrowingTreeStrategy.Random:
                    var randomIndex = _randomValueGenerator.GetNext(0, activeCells.Count - 1);
                    return activeCells.ElementAt(randomIndex);
                case GrowingTreeStrategy.RandomOldest:
                    var randomFirstIndex = _randomValueGenerator.GetNext(middle, activeCells.Count - 1);
                    return activeCells.ElementAt(randomFirstIndex);
                case GrowingTreeStrategy.RandomNewest:
                    var randomLastIndex = _randomValueGenerator.GetNext(0, middle);
                    return activeCells.ElementAt(randomLastIndex);
                default:
                    throw new ArgumentException("Unsupported Cell Selection Strategy");
            }
        }
    }
}

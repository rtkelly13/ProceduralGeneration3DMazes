using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public class GrowingTreeAlgorithmLinkedList : IGrowingTreeAlgorithm
    {
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IRandomValueGenerator _randomValueGenerator;
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public GrowingTreeAlgorithmLinkedList(IRandomPointGenerator randomPointGenerator, 
            IRandomValueGenerator randomValueGenerator, 
            IDirectionsFlagParser directionsFlagParser)
        {
            _randomPointGenerator = randomPointGenerator;
            _randomValueGenerator = randomValueGenerator;
            _directionsFlagParser = directionsFlagParser;
        }

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            var growingTreeSettings = settings.GrowingTreeSettings;
            if (growingTreeSettings == null)
            {
                throw new ArgumentException("The correct settings are not present");
            }
            return GenerateMaze(maze, growingTreeSettings.Strategies);
        }

        private AlgorithmRunResults GenerateMaze(IMazeCarver maze, List<GrowingTreeStrategy> strategies)
        {
            var pointsAndDirections = new List<DirectionAndPoint>();
            var heatmap = new Dictionary<MazePoint, int>();
            var randomPoint = _randomPointGenerator.RandomPoint(maze.Size);
            var activeCells = new LinkedList<MazePoint>();
            activeCells.AddFirst(randomPoint);
            while (activeCells.Any())
            {
                var currentPoint = GetNextPoint(activeCells, strategies);
                heatmap[currentPoint] = heatmap.GetValueOrDefault(currentPoint) + 1;

                maze.JumpToPoint(currentPoint);
                var carvableDirections = maze.CarvableDirections();
                ArrayHelper.Shuffle(carvableDirections);
                var carved = false;
                foreach (var direction in carvableDirections)
                {
                    maze.JumpInDirection(direction);
                    var carvedDirections = maze.AlreadyCarvedDirections();
                    if (carvedDirections.Length == 0)
                    {
                        pointsAndDirections.Add(new DirectionAndPoint { Direction = direction, MazePoint = currentPoint });
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
            return new AlgorithmRunResults
            {
                Carver = maze,
                DirectionsCarvedIn = pointsAndDirections,
                Heatmap = heatmap
            };
        }

        private MazePoint GetNextPoint(LinkedList<MazePoint> activeCells, List<GrowingTreeStrategy> strategies)
        {
            var item = _randomValueGenerator.GetNext(0, strategies.Count - 1);
            var strategy = strategies[item];
            var middle = activeCells.Count / 2;
            switch (strategy)
            {
                case GrowingTreeStrategy.Oldest:
                    return activeCells.First!.Value;
                case GrowingTreeStrategy.Newest:
                    return activeCells.Last!.Value;
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

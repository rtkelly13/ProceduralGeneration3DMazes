using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class RecursiveBacktrackerAlgorithm: IRecursiveBacktrackerAlgorithm
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IArrayHelper _arrayHelper;

        public RecursiveBacktrackerAlgorithm(IDirectionsFlagParser directionsFlagParser, IRandomPointGenerator randomPointGenerator, IArrayHelper arrayHelper)
        {
            _directionsFlagParser = directionsFlagParser;
            _randomPointGenerator = randomPointGenerator;
            _arrayHelper = arrayHelper;
        }

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            var randomPoint = _randomPointGenerator.RandomPoint(maze.Size);
            maze.JumpToPoint(randomPoint);
            return RecursiveBackTracker(maze);
            
        }

        public AlgorithmRunResults RecursiveBackTracker(IMazeCarver carver)
        {
            var pointsAndDirections = new List<DirectionAndPoint>();
            var directions = carver.CarvableDirections().ToList();
            _arrayHelper.Shuffle(directions);
            var currentPoint = carver.CurrentPoint;
            foreach (var direction in directions)
            {
                carver.JumpInDirection(direction);
                var carvedDirections = carver.AlreadyCarvedDirections();
                if (!carvedDirections.Any())
                {
                    pointsAndDirections.Add(new DirectionAndPoint { Direction = direction, MazePoint = currentPoint });
                    var oppositeDirection = _directionsFlagParser.OppositeDirection(direction);
                    carver.CarveInDirection(oppositeDirection);
                    RecursiveBackTracker(carver);
                }
                carver.JumpToPoint(currentPoint);
            }
            return new AlgorithmRunResults
            {
                Carver = carver,
                DirectionsCarvedIn = pointsAndDirections
            };
        }
    }
}

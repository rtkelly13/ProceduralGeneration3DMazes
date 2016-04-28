using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class BacktrackerAlgorithm: IRecursiveBacktrackerAlgorithm
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IRandomPointGenerator _randomPointGenerator;

        public BacktrackerAlgorithm(IDirectionsFlagParser directionsFlagParser, IRandomPointGenerator randomPointGenerator)
        {
            _directionsFlagParser = directionsFlagParser;
            _randomPointGenerator = randomPointGenerator;
        }

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            var pointAndDirection = new List<DirectionAndPoint>();
            var randomPoint = _randomPointGenerator.RandomPoint(maze.Size);
            maze.JumpToPoint(randomPoint);
            var activeCells = new LinkedList<MazePoint>();
            activeCells.AddLast(randomPoint);
            while (activeCells.Any())
            {
                var currentPoint = activeCells.Last.Value;
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
                        pointAndDirection.Add(new DirectionAndPoint { Direction = direction, MazePoint = currentPoint});
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
                    activeCells.RemoveLast();
                }
            }
            return new AlgorithmRunResults
            {
                Carver = maze,
                DirectionsCarvedIn = pointAndDirection
            };
        }
    }
}
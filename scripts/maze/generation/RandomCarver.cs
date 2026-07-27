using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public class RandomCarver : IRandomCarver
    {
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IRandomValueGenerator _randomValueGenerator;

        public RandomCarver(IRandomPointGenerator randomPointGenerator, 
            IPointsAndDirectionsRetriever pointsAndDirectionsRetriever, 
            IDirectionsFlagParser directionsFlagParser,
            IRandomValueGenerator randomValueGenerator)
        {
            _randomPointGenerator = randomPointGenerator;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
            _directionsFlagParser = directionsFlagParser;
            _randomValueGenerator = randomValueGenerator;
        }

        public void CarveRandomWalls(IMazeCarver carver, WallCarverOption option, int numberOfWalls)
        {
            switch (option)
            {
                case WallCarverOption.None:
                    break;
                case WallCarverOption.Random:
                    RandomCarveWalls(carver, numberOfWalls);
                    break;
                case WallCarverOption.DeadEnd:
                    DeadEndCarver(carver, numberOfWalls, false);
                    break;
                case WallCarverOption.DeadEndWithPreferredDirection:
                    DeadEndCarver(carver, numberOfWalls, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DeadEndCarver(IMazeCarver carver, int numberOfWalls, bool hasPreferredDirection)
        {
            var pointsAndDirections = _pointsAndDirectionsRetriever.GetDeadEnds(carver).ToList();
            _randomValueGenerator.Shuffle(pointsAndDirections);
            foreach (var pointAndDirections in pointsAndDirections)
            {
                if (numberOfWalls > 0)
                {
                    Direction preferredDirection = Direction.None;
                    if (hasPreferredDirection)
                    {
                        preferredDirection = _directionsFlagParser
                                                    .OppositeDirection(pointAndDirections.Directions[0]);
                    }
                    numberOfWalls = CheckPoint(pointAndDirections.Point, carver, numberOfWalls, preferredDirection);
                }
                else
                {
                    break;
                }
            }
            RandomCarveWalls(carver, numberOfWalls);
        }

        private void RandomCarveWalls(IMazeCarver carver, int numberOfWalls)
        {
            while (numberOfWalls > 0)
            {
                numberOfWalls = CheckPoint(_randomPointGenerator.RandomPoint(carver.Size), carver, numberOfWalls);
            }
        }

        private int CheckPoint(MazePoint point, IMazeCarver carver, int numberOfWalls, Direction preferredDirection = Direction.None)
        {
            carver.JumpToPoint(point);
            var directions = carver.CarvableDirections();
            _randomValueGenerator.Shuffle(directions);
            if (directions.Length > 0)
            {
                var selectedDirection = directions.Contains(preferredDirection)
                    ? preferredDirection
                    : directions[0];
                carver.CarveInDirection(selectedDirection);
                numberOfWalls--;
            }
            return numberOfWalls;
        }
    }
}

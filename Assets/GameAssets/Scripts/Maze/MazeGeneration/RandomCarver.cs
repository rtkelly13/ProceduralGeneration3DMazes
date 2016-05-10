using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Helper;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class RandomCarver : IRandomCarver
    {
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IArrayHelper _arrayHelper;

        public RandomCarver(IRandomPointGenerator randomPointGenerator, IPointsAndDirectionsRetriever pointsAndDirectionsRetriever, IDirectionsFlagParser directionsFlagParser, IArrayHelper arrayHelper)
        {
            _randomPointGenerator = randomPointGenerator;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
            _directionsFlagParser = directionsFlagParser;
            _arrayHelper = arrayHelper;
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
            _arrayHelper.Shuffle(pointsAndDirections);
            foreach (var pointAndDirections in pointsAndDirections)
            {
                if (numberOfWalls > 0)
                {
                    Direction preferredDirection = Direction.None;
                    if (hasPreferredDirection)
                    {
                        preferredDirection = _directionsFlagParser
                                                    .OppositeDirection(pointAndDirections.Directions.First());
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
            var directions = carver.CarvableDirections().ToList();
            _arrayHelper.Shuffle(directions);
            if (directions.Any())
            {
                var selectedDirection = directions.Contains(preferredDirection)
                    ? preferredDirection
                    : directions.First();
                carver.CarveInDirection(selectedDirection);
                numberOfWalls--;
            }
            return numberOfWalls;
        }
    }
}

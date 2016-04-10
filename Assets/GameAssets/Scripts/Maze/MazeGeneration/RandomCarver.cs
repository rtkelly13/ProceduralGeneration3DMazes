using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Helper;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class RandomCarver : IRandomCarver
    {
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;

        public RandomCarver(IRandomPointGenerator randomPointGenerator, IPointsAndDirectionsRetriever pointsAndDirectionsRetriever)
        {
            _randomPointGenerator = randomPointGenerator;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
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
                    var pointsAndDirections = _pointsAndDirectionsRetriever.GetDeadEnds(carver).ToList();
                    pointsAndDirections.Shuffle();
                    foreach (var pointAndDirections in pointsAndDirections)
                    {
                        if (numberOfWalls > 0)
                        {
                            numberOfWalls = CheckPoint(pointAndDirections.Point, carver, numberOfWalls);
                        }
                        else
                        {
                            break;
                        }
                    }
                    RandomCarveWalls(carver, numberOfWalls);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }



        private void RandomCarveWalls(IMazeCarver carver, int numberOfWalls)
        {
            while (numberOfWalls > 0)
            {
                numberOfWalls = CheckPoint(_randomPointGenerator.RandomPoint(carver.Size), carver, numberOfWalls);
            }
        }

        private int CheckPoint(MazePoint point, IMazeCarver carver, int numberOfWalls)
        {
            carver.JumpToPoint(point);
            var directions = carver.CarvableDirections().ToList();
            directions.Shuffle();
            if (directions.Any())
            {
                carver.CarveInDirection(directions.First());
                numberOfWalls--;
            }
            return numberOfWalls;
        }
    }
}

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
        private readonly IDeadEndRetriever _deadEndRetriever;

        public RandomCarver(IRandomPointGenerator randomPointGenerator, IDeadEndRetriever deadEndRetriever)
        {
            _randomPointGenerator = randomPointGenerator;
            _deadEndRetriever = deadEndRetriever;
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
                    var points = _deadEndRetriever.GetDeadEnds(carver).ToList();
                    points.Shuffle();
                    foreach (var point in points)
                    {
                        if (numberOfWalls > 0)
                        {
                            numberOfWalls = CheckPoint(point, carver, numberOfWalls);
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

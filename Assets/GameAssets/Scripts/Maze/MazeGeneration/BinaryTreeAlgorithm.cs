using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class BinaryTreeAlgorithm : IBinaryTreeAlgorithm
    {
        private readonly IMazeHelper _mazeHelper;
        private readonly List<Direction> _directions; 

        public BinaryTreeAlgorithm(IMazeHelper mazeHelper)
        {
            _mazeHelper = mazeHelper;
            _directions = new List<Direction>() { Direction.Left, Direction.Up, Direction.Forward };
        }

        public IMazeCarver GenerateMaze(IMazeCarver carver, MazeGenerationSettings settings)
        {
            _mazeHelper.DoForEachPoint(carver.Size, p =>
            {
                carver.JumpToPoint(p);
                var directions = _directions.Where(carver.CanCarveInDirection).ToList();
                directions.Shuffle();
                if (directions.Any())
                {
                    var first = directions.First();
                    carver.CarveInDirection(first);
                }
            });
            return carver;
        }
    }

    public interface IBinaryTreeAlgorithm: IMazeGenerationAlgorithm
    {
    }
}

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

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            _mazeHelper.DoForEachPoint(maze.Size, p =>
            {
                maze.JumpToPoint(p);
                var directions = _directions.Where(maze.CanCarveInDirection).ToList();
                directions.Shuffle();
                if (directions.Any())
                {
                    var first = directions.First();
                    maze.CarveInDirection(first);
                }
            });
            return new AlgorithmRunResults
            {
                Carver = maze,
            };
        }
    }

    public interface IBinaryTreeAlgorithm: IMazeGenerationAlgorithm
    {
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class BinaryTreeAlgorithm : IBinaryTreeAlgorithm
    {
        private readonly IMazeHelper _mazeHelper;
        private readonly IArrayHelper _arrayHelper;
        private readonly List<Direction> _directions; 

        public BinaryTreeAlgorithm(IMazeHelper mazeHelper, IArrayHelper arrayHelper)
        {
            _mazeHelper = mazeHelper;
            _arrayHelper = arrayHelper;
            _directions = new List<Direction>() { Direction.Left, Direction.Up, Direction.Forward };
        }

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            var pointsAndDirections = new List<DirectionAndPoint>();
            _mazeHelper.DoForEachPoint(maze.Size, p =>
            {
                maze.JumpToPoint(p);
                var directions = _directions.Where(maze.CanCarveInDirection).ToList();
                _arrayHelper.Shuffle(directions);
                if (directions.Any())
                {
                    var first = directions.First();
                    pointsAndDirections.Add(new DirectionAndPoint { Direction = first, MazePoint = maze.CurrentPoint});
                    maze.CarveInDirection(first);
                }
            });
            
            return new AlgorithmRunResults
            {
                Carver = maze,
                DirectionsCarvedIn = pointsAndDirections
            };
        }
    }
}

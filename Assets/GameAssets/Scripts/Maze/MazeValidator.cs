using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze
{
    public class MazeValidator : IMazeValidator
    {
        private readonly IMazeHelper _mazeHelper;

        public MazeValidator(IMazeHelper mazeHelper)
        {
            _mazeHelper = mazeHelper;
        }

        public bool EveryPointHasDirection(IMazeJumper maze)
        {
            var results = _mazeHelper.GetForEachPoint<CellValidationResult>(maze.Size, x => GetResult(maze, x)).ToList();
            var validationFailures = results.Where(x => x.Flag == Direction.None).ToList();
            if (!validationFailures.Any())
            {
                return true;
            }
            //var first = validationFailures.First();
            return false;
        }

        private CellValidationResult GetResult(IMazeJumper maze, MazePoint point)
        {
            maze.JumpToPoint(point);
            var flag = maze.GetFlagFromPoint();
            return new CellValidationResult
            {
                CellValid = flag != Direction.None,
                Flag = flag,
                Point = maze.CurrentPoint
            };
        }
    }
}

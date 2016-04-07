using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class DeadEndRetriever : IDeadEndRetriever
    {
        private readonly IMazeHelper _mazeHelper;

        public DeadEndRetriever(IMazeHelper mazeHelper)
        {
            _mazeHelper = mazeHelper;
        }

        public IEnumerable<MazePoint> GetDeadEnds(IMazeCarver mazeCarver)
        {
            return _mazeHelper.GetPoints(mazeCarver.Size, point =>
            {
                mazeCarver.JumpToPoint(point);
                var directions = mazeCarver.GetsDirectionsFromPoint();
                if (directions.Count() == 1)
                {
                    return true;
                }
                return false;
            });
        } 
    }
}

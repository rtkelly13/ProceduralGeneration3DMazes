using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
{
    public class MazeStatsGenerator : IMazeStatsGenerator
    {
        private readonly IMazeHelper _mazeHelper;
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public MazeStatsGenerator(IMazeHelper mazeHelper, IDirectionsFlagParser directionsFlagParser)
        {
            _mazeHelper = mazeHelper;
            _directionsFlagParser = directionsFlagParser;
        }

        public MazeStatsResults GetResultsFromMaze(IMazeCarver mazeCarver)
        {
            mazeCarver.SetState(ModelMode.Standard);

            var standardResult = GetResult(mazeCarver);

            mazeCarver.SetState(ModelMode.DeadEndFilled);

            var deadEndResult = GetResult(mazeCarver);

            mazeCarver.SetState(ModelMode.Standard);

            return new MazeStatsResults
            {
                Standard = standardResult,
                DeadEnd = deadEndResult
            };

        }

        private MazeStatsResult GetResult(IMazeCarver mazeCarver)
        {
            var directionsUsed = _directionsFlagParser.Directions
                    .ToDictionary(direction => direction, direction => 0);
            _mazeHelper.DoForEachPoint(mazeCarver.Size, p =>
            {
                mazeCarver.JumpToPoint(p);
                foreach (var direction in mazeCarver.GetsDirectionsFromPoint())
                {
                    directionsUsed[direction]++;
                }
            });

            var maximumUsed = directionsUsed.OrderBy(x => x.Value).First();
            var minimumUsed = directionsUsed.OrderByDescending(x => x.Value).First();
            return new MazeStatsResult
            {
                DirectionsUsed = directionsUsed,
                MaximumUse = new DirectionResult
                {
                    Direction = maximumUsed.Key,
                    NumberOfUsages = minimumUsed.Value
                },
                MinimumUse = new DirectionResult
                {
                    Direction = minimumUsed.Key,
                    NumberOfUsages = minimumUsed.Value
                }
            };
        } 
    }

}

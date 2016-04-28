using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.Maze.Heuristics
{
    public class MazeStatsGenerator : IMazeStatsGenerator
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public MazeStatsGenerator(IDirectionsFlagParser directionsFlagParser)
        {
            _directionsFlagParser = directionsFlagParser;
        }

        public MazeStatsResult GetResultsFromMaze(AlgorithmRunResults results)
        {
            return GetResult(results.DirectionsCarvedIn);
        }

        private MazeStatsResult GetResult(List<DirectionAndPoint> pointsAndDirections)
        {
            var directionsUsed = _directionsFlagParser.Directions.ToDictionary(
                direction => direction,
                direction => pointsAndDirections.Count(x => x.Direction == direction));

            var directions = directionsUsed.OrderBy(x => x.Value).ToList();
            var maximumUsed = directions.Last();
            var minimumUsed = directions.First();
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

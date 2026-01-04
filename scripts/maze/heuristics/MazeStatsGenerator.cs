using System.Linq;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Generation;

namespace ProceduralMaze.Maze.Heuristics
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

        private MazeStatsResult GetResult(System.Collections.Generic.List<Agents.DirectionAndPoint> pointsAndDirections)
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
                    NumberOfUsages = maximumUsed.Value
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

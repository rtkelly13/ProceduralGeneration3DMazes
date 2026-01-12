using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public class PrimsAlgorithm : IPrimsAlgorithm
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IRandomPointGenerator _randomPointGenerator;
        private readonly IRandomValueGenerator _randomValueGenerator;

        public PrimsAlgorithm(
            IDirectionsFlagParser directionsFlagParser,
            IRandomPointGenerator randomPointGenerator,
            IRandomValueGenerator randomValueGenerator)
        {
            _directionsFlagParser = directionsFlagParser;
            _randomPointGenerator = randomPointGenerator;
            _randomValueGenerator = randomValueGenerator;
        }

        public AlgorithmRunResults GenerateMaze(IMazeCarver maze, MazeGenerationSettings settings)
        {
            var pointsAndDirections = new List<DirectionAndPoint>();
            var heatmap = new Dictionary<MazePoint, int>();
            var visited = new HashSet<MazePoint>();
            
            // Start with a random point
            var startPoint = _randomPointGenerator.RandomPoint(maze.Size);
            visited.Add(startPoint);
            
            // Frontier is a list of potential edges to carve: (from, direction to target)
            var frontier = new List<(MazePoint from, Direction direction)>();
            AddFrontier(maze, startPoint, visited, frontier);

            while (frontier.Any())
            {
                // Randomly select an edge from the frontier
                int index = _randomValueGenerator.GetNext(0, frontier.Count - 1);
                var edge = frontier[index];
                frontier.RemoveAt(index);

                maze.JumpToPoint(edge.from);
                maze.JumpInDirection(edge.direction);
                var targetPoint = maze.CurrentPoint;

                if (!visited.Contains(targetPoint))
                {
                    // Carve the connection
                    maze.JumpToPoint(edge.from);
                    maze.CarveInDirection(edge.direction);
                    pointsAndDirections.Add(new DirectionAndPoint { Direction = edge.direction, MazePoint = edge.from });

                    var oppositeDirection = _directionsFlagParser.OppositeDirection(edge.direction);
                    maze.JumpToPoint(targetPoint);
                    maze.CarveInDirection(oppositeDirection);

                    visited.Add(targetPoint);
                    heatmap[targetPoint] = heatmap.GetValueOrDefault(targetPoint) + 1;
                    heatmap[edge.from] = heatmap.GetValueOrDefault(edge.from) + 1;

                    // Add new potential edges to the frontier
                    AddFrontier(maze, targetPoint, visited, frontier);
                }
            }

            return new AlgorithmRunResults
            {
                Carver = maze,
                DirectionsCarvedIn = pointsAndDirections,
                Heatmap = heatmap
            };
        }

        private void AddFrontier(IMazeCarver maze, MazePoint point, HashSet<MazePoint> visited, List<(MazePoint from, Direction direction)> frontier)
        {
            maze.JumpToPoint(point);
            var carvableDirections = maze.CarvableDirections();
            foreach (var direction in carvableDirections)
            {
                maze.JumpToPoint(point);
                maze.JumpInDirection(direction);
                if (!visited.Contains(maze.CurrentPoint))
                {
                    frontier.Add((point, direction));
                }
            }
        }
    }
}

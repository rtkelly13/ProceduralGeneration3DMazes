using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Solver
{
    public class GraphBuilder : IGraphBuilder
    {
        private readonly IPointsAndDirectionsRetriever _directionsRetriever;
        private readonly IDirectionsFlagParser _directionsFlagParser;

        public GraphBuilder(IPointsAndDirectionsRetriever directionsRetriever, IDirectionsFlagParser directionsFlagParser)
        {
            _directionsRetriever = directionsRetriever;
            _directionsFlagParser = directionsFlagParser;
        }

        public Graph GetGraphFromMaze(IMazeCarver carver)
        {
            carver.SetState(ModelMode.DeadEndFilled);

            var dictionary = new Dictionary<MazePoint, GraphNode>();
            var junctions = _directionsRetriever.GetJunctions(carver).ToList();

            foreach (var junction in junctions)
            {
                var graphNode = new GraphNode
                {
                    Edges = GetGraphEdges(carver, junction).ToList(),
                    ShortestPath = int.MaxValue,
                    ShortestPathDirections = new List<Direction>()
                };
                dictionary.Add(junction.Point, graphNode);
            }

            if (!dictionary.ContainsKey(carver.StartPoint))
            {
                carver.JumpToPoint(carver.StartPoint);
                var junction = new PointAndDirections
                {
                    Point = carver.CurrentPoint,
                    Directions = carver.GetsDirectionsFromPoint().ToList(), 
                };
                var graphNode = new GraphNode
                {
                    Edges = GetGraphEdges(carver, junction).ToList(),
                    ShortestPath = int.MaxValue,
                    ShortestPathDirections = new List<Direction>()
                };
                dictionary.Add(carver.StartPoint, graphNode);
            }

            if (!dictionary.ContainsKey(carver.EndPoint))
            {
                carver.JumpToPoint(carver.EndPoint);
                var junction = new PointAndDirections
                {
                    Point = carver.CurrentPoint,
                    Directions = carver.GetsDirectionsFromPoint().ToList()
                };
                var graphNode = new GraphNode
                {
                    Edges = GetGraphEdges(carver, junction).ToList(),
                    ShortestPath = int.MaxValue,
                    ShortestPathDirections = new List<Direction>()
                };
                dictionary.Add(carver.EndPoint, graphNode);
            }

            

            carver.SetState(ModelMode.Standard);
            return new Graph
            {
                Nodes = dictionary,
            };
        }

        private IEnumerable<GraphEdge> GetGraphEdges(IMazeCarver carver, PointAndDirections junction)
        {
            foreach (var direction in junction.Directions)
            {
                var result = new GraphEdge
                {
                    DirectionsToPoint = new List<Direction>() { direction }
                };
                carver.JumpToPoint(junction.Point);
                carver.JumpInDirection(direction);
                Direction inputDirection = direction;
                bool endFound = false;
                do
                {
                    var directions = carver.GetsDirectionsFromPoint().ToList();
                    if (IsStartOrEndPointOrJunction(carver, directions))
                    {
                        result.Point = carver.CurrentPoint;
                        endFound = true;
                    }
                    else
                    {
                        var oppositeDirection = _directionsFlagParser.OppositeDirection(inputDirection);
                        directions = directions.Where(x => x != oppositeDirection).ToList();
                        inputDirection = directions.Single();
                        result.DirectionsToPoint.Add(inputDirection);
                        carver.JumpInDirection(inputDirection);
                    }
                } while (!endFound);
                yield return result;
            }
        }

        private bool IsStartOrEndPointOrJunction(IMazeCarver carver, List<Direction> directions)
        {
            return _directionsRetriever.PointIsStartOrEnd(carver.CurrentPoint, carver.StartPoint, carver.EndPoint) ||
                   _directionsRetriever.IsJunction(directions);
        }

    }
}

using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
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

        public Graph GetGraphFromMaze(IMazeJumper jumper)
        {
            jumper.SetState(ModelMode.DeadEndFilled);

            var dictionary = new Dictionary<MazePoint, GraphNode>();
            var junctions = _directionsRetriever.GetJunctions(jumper);

            foreach (var junction in junctions)
            {
                var graphNode = new GraphNode
                {
                    Point = junction.Point,
                    Edges = GetGraphEdges(jumper, junction),
                    ShortestPath = int.MaxValue,
                    ShortestPathDirections = []
                };
                dictionary.Add(junction.Point, graphNode);
            }

            if (!dictionary.ContainsKey(jumper.StartPoint))
            {
                jumper.JumpToPoint(jumper.StartPoint);
                var junction = new PointAndDirections
                {
                    Point = jumper.CurrentPoint,
                    Directions = jumper.GetDirectionsFromPoint(),
                };
                var graphNode = new GraphNode
                {
                    Point = junction.Point,
                    Edges = GetGraphEdges(jumper, junction),
                    ShortestPath = int.MaxValue,
                    ShortestPathDirections = []
                };
                dictionary.Add(jumper.StartPoint, graphNode);
            }

            if (!dictionary.ContainsKey(jumper.EndPoint))
            {
                jumper.JumpToPoint(jumper.EndPoint);
                var junction = new PointAndDirections
                {
                    Point = jumper.CurrentPoint,
                    Directions = jumper.GetDirectionsFromPoint()
                };
                var graphNode = new GraphNode
                {
                    Point = junction.Point,
                    Edges = GetGraphEdges(jumper, junction),
                    ShortestPath = int.MaxValue,
                    ShortestPathDirections = []
                };
                dictionary.Add(jumper.EndPoint, graphNode);
            }

            jumper.SetState(ModelMode.Standard);
            return new Graph
            {
                Nodes = dictionary,
            };
        }

        private GraphEdge[] GetGraphEdges(IMazeJumper jumper, PointAndDirections junction)
        {
            var edges = new List<GraphEdge>(junction.Directions.Length);
            
            foreach (var direction in junction.Directions)
            {
                var directionsToPoint = new List<Direction> { direction };
                jumper.JumpToPoint(junction.Point);
                jumper.JumpInDirection(direction);
                Direction inputDirection = direction;
                bool endFound = false;
                MazePoint endPoint = default!;
                
                do
                {
                    var directions = jumper.GetDirectionsFromPoint();
                    if (IsStartOrEndPointOrJunction(jumper, directions))
                    {
                        endPoint = jumper.CurrentPoint;
                        endFound = true;
                    }
                    else
                    {
                        var oppositeDirection = _directionsFlagParser.OppositeDirection(inputDirection);
                        Direction? nextDir = null;
                        foreach (var d in directions)
                        {
                            if (d != oppositeDirection)
                            {
                                nextDir = d;
                                break;
                            }
                        }
                        inputDirection = nextDir!.Value;
                        directionsToPoint.Add(inputDirection);
                        jumper.JumpInDirection(inputDirection);
                    }
                } while (!endFound);
                
                edges.Add(new GraphEdge
                {
                    Point = endPoint,
                    DirectionsToPoint = [.. directionsToPoint]
                });
            }
            
            return [.. edges];
        }

        private bool IsStartOrEndPointOrJunction(IMazeJumper jumper, Direction[] directions)
        {
            return _directionsRetriever.PointIsStartOrEnd(jumper.CurrentPoint, jumper.StartPoint, jumper.EndPoint) ||
                   _directionsRetriever.IsJunction(directions);
        }
    }
}

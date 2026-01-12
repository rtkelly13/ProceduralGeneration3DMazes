using System.Collections.Generic;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver.Heuristics;

namespace ProceduralMaze.Maze.Solver
{
    public class AStarSolver : IShortestPathSolver
    {
        private readonly IGraphBuilder _graphBuilder;
        private readonly IHeuristicStrategy _heuristic;

        public AStarSolver(IGraphBuilder graphBuilder, IHeuristicStrategy heuristic)
        {
            _graphBuilder = graphBuilder;
            _heuristic = heuristic;
        }

        public ShortestPathResult GetGraph(IMazeJumper jumper)
        {
            var graph = _graphBuilder.GetGraphFromMaze(jumper);
            var startPoint = jumper.StartPoint;
            var endPoint = jumper.EndPoint;

            graph.Nodes[startPoint].ShortestPath = 0;

            var visited = new HashSet<MazePoint>();
            var priorityQueue = new PriorityQueue<GraphNode, double>();

            // Find start node
            var startNode = graph.Nodes[startPoint];
            // fScore = gScore + hScore. For start, gScore = 0.
            priorityQueue.Enqueue(startNode, _heuristic.Calculate(startPoint, endPoint));

            while (priorityQueue.Count > 0)
            {
                var currentNode = priorityQueue.Dequeue();

                // If we reached the target, we found the shortest path
                if (currentNode.Point.Equals(endPoint))
                {
                    break;
                }

                // Skip if already visited with a better path
                if (!visited.Add(currentNode.Point))
                {
                    continue;
                }

                // Process all edges from this node
                foreach (var edge in currentNode.Edges)
                {
                    var targetNode = graph.Nodes[edge.Point];
                    var gScore = currentNode.ShortestPath + edge.DirectionsToPoint.Length;

                    if (gScore < targetNode.ShortestPath)
                    {
                        targetNode.ShortestPath = gScore;
                        targetNode.ShortestPathDirections = [.. currentNode.ShortestPathDirections, .. edge.DirectionsToPoint];

                        // fScore = gScore + heuristic(current -> goal)
                        double fScore = gScore + _heuristic.Calculate(edge.Point, endPoint);
                        priorityQueue.Enqueue(targetNode, fScore);
                    }
                }
            }

            var endNode = graph.Nodes[endPoint];
            return new ShortestPathResult
            {
                Graph = graph,
                ShortestPath = endNode.ShortestPath,
                ShortestPathDirections = endNode.ShortestPathDirections
            };
        }
    }
}

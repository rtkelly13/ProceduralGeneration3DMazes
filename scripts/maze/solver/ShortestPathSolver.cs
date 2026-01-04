using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    public class ShortestPathSolver : IShortestPathSolver
    {
        private readonly IGraphBuilder _graphBuilder;

        /// <summary>
        /// Threshold for switching from simple O(n²) algorithm to PriorityQueue O(n log n).
        /// Based on benchmarks, PriorityQueue overhead is only worthwhile for larger graphs.
        /// Tuned to ~1500 nodes where the algorithms have similar performance.
        /// </summary>
        private const int PriorityQueueThreshold = 1500;

        public ShortestPathSolver(IGraphBuilder graphBuilder)
        {
            _graphBuilder = graphBuilder;
        }

        public ShortestPathResult GetGraph(IMazeJumper jumper)
        {
            var graph = _graphBuilder.GetGraphFromMaze(jumper);
            graph.Nodes[jumper.StartPoint].ShortestPath = 0;

            // Use PriorityQueue for larger graphs where O(n log n) beats O(n²)
            if (graph.Nodes.Count >= PriorityQueueThreshold)
            {
                ProcessGraphWithPriorityQueue(graph);
            }
            else
            {
                ProcessGraphSimple(graph);
            }

            var node = graph.Nodes[jumper.EndPoint];
            return new ShortestPathResult
            {
                Graph = graph,
                ShortestPath = node.ShortestPath,
                ShortestPathDirections = node.ShortestPathDirections
            };
        }

        /// <summary>
        /// Simple O(n²) Dijkstra implementation.
        /// Faster for small graphs due to lower overhead.
        /// </summary>
        private void ProcessGraphSimple(Graph graph)
        {
            bool finished = false;
            var queue = graph.Nodes.Values.ToList();
            while (!finished)
            {
                GraphNode? nextNode = queue.OrderBy(n => n.ShortestPath).FirstOrDefault(
                    n => int.MaxValue != n.ShortestPath);
                if (nextNode != null)
                {
                    ProcessNode(nextNode, graph);
                    queue.Remove(nextNode);
                }
                else
                {
                    finished = true;
                }
            }
        }

        /// <summary>
        /// PriorityQueue-based O(n log n) Dijkstra implementation.
        /// More efficient for large graphs despite data structure overhead.
        /// </summary>
        private void ProcessGraphWithPriorityQueue(Graph graph)
        {
            var visited = new HashSet<MazePoint>();
            var priorityQueue = new PriorityQueue<GraphNode, int>();

            // Find and enqueue the start node (the one with ShortestPath = 0)
            foreach (var node in graph.Nodes.Values)
            {
                if (node.ShortestPath == 0)
                {
                    priorityQueue.Enqueue(node, 0);
                    break;
                }
            }

            while (priorityQueue.Count > 0)
            {
                var currentNode = priorityQueue.Dequeue();

                // Skip if already visited (we may have enqueued duplicates with different priorities)
                if (!visited.Add(currentNode.Point))
                {
                    continue;
                }

                // Process all edges from this node
                foreach (var edge in currentNode.Edges)
                {
                    var targetNode = graph.Nodes[edge.Point];
                    var newShortestPath = currentNode.ShortestPath + edge.DirectionsToPoint.Length;

                    if (newShortestPath < targetNode.ShortestPath)
                    {
                        targetNode.ShortestPath = newShortestPath;
                        targetNode.ShortestPathDirections
                            = [.. currentNode.ShortestPathDirections, .. edge.DirectionsToPoint];

                        // Enqueue with updated priority (duplicates are handled by visited check)
                        priorityQueue.Enqueue(targetNode, newShortestPath);
                    }
                }
            }
        }

        private void ProcessNode(GraphNode node, Graph graph)
        {
            foreach (var edge in node.Edges)
            {
                var targetNode = graph.Nodes[edge.Point];
                var newShortestPath = node.ShortestPath + edge.DirectionsToPoint.Length;
                if (newShortestPath < targetNode.ShortestPath)
                {
                    targetNode.ShortestPath = newShortestPath;
                    targetNode.ShortestPathDirections
                        = [.. node.ShortestPathDirections, .. edge.DirectionsToPoint];
                }
            }
        }
    }

    public class ShortestPathResult
    {
        public Graph Graph { get; set; } = null!;
        public Direction[] ShortestPathDirections { get; set; } = [];
        public int ShortestPath { get; set; }
    }
}

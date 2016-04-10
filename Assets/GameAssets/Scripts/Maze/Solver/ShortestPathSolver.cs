using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Solver
{
    public class ShortestPathSolver : IShortestPathSolver
    {
        private readonly IGraphBuilder _graphBuilder;

        public ShortestPathSolver(IGraphBuilder graphBuilder)
        {
            _graphBuilder = graphBuilder;
        }

        public ShortestPathResult GetGraph(IMazeCarver carver)
        {
            var graph = _graphBuilder.GetGraphFromMaze(carver);
            graph.Nodes[carver.StartPoint].ShortestPath = 0;
            ProcessGraph(graph);
            var node = graph.Nodes[carver.EndPoint];
            return new ShortestPathResult
            {
                Graph = graph,
                ShortestPath = node.ShortestPath,
                ShortestPathDirections = node.ShortestPathDirections
            };
        }

        private void ProcessGraph(Graph graph)
        {
            bool finished = false;
            var queue = graph.Nodes.Values.ToList();
            while (!finished)
            {
                GraphNode nextNode = queue.OrderBy(n => n.ShortestPath).FirstOrDefault(
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

        private void ProcessNode(GraphNode node, Graph graph)
        {
            foreach (var edge in node.Edges)
            {
                var targetNode = graph.Nodes[edge.Point];
                var newShortestPath = node.ShortestPath + edge.DirectionsToPoint.Count;
                if (newShortestPath < targetNode.ShortestPath)
                {
                    targetNode.ShortestPath = newShortestPath;
                    targetNode.ShortestPathDirections 
                        = node.ShortestPathDirections.Concat(edge.DirectionsToPoint).ToList();
                }
            }
        }
    }

    public class ShortestPathResult
    {
        public Graph Graph { get; set; }
        public List<Direction> ShortestPathDirections { get; set; }
        public int ShortestPath { get; set; }
    }
}

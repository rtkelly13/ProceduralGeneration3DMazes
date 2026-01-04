using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    /// <summary>
    /// Implements Yen's K-shortest paths algorithm to find multiple paths through a maze.
    /// </summary>
    public class KShortestPathsSolver : IKShortestPathsSolver
    {
        private readonly IGraphBuilder _graphBuilder;

        public KShortestPathsSolver(IGraphBuilder graphBuilder)
        {
            _graphBuilder = graphBuilder;
        }

        public List<PathResult> GetKShortestPaths(IMazeJumper jumper, int k)
        {
            var graph = _graphBuilder.GetGraphFromMaze(jumper);
            return GetKShortestPaths(graph, jumper.StartPoint, jumper.EndPoint, k);
        }

        public List<PathResult> GetKShortestPaths(Graph graph, MazePoint startPoint, MazePoint endPoint, int k)
        {
            if (k <= 0) return new List<PathResult>();

            // Get the first shortest path using Dijkstra
            var firstPath = ComputeShortestPath(graph, startPoint, endPoint, new HashSet<(MazePoint, MazePoint)>());
            if (firstPath == null)
            {
                return new List<PathResult>();
            }

            var results = new List<PathResult> { CreatePathResult(firstPath, graph, startPoint, 0, firstPath.Count) };

            // For perfect mazes (no loops), there's only one path
            if (k == 1 || !HasAlternativePaths(graph))
            {
                return results;
            }

            // Yen's algorithm: find K-1 more paths
            var candidates = new SortedSet<CandidatePath>(new CandidatePathComparer());

            for (int pathIndex = 1; pathIndex < k; pathIndex++)
            {
                var previousPath = GetPathNodes(results[pathIndex - 1], startPoint);

                // Try each node in the previous path as a spur node
                for (int spurIndex = 0; spurIndex < previousPath.Count - 1; spurIndex++)
                {
                    var spurNode = previousPath[spurIndex];
                    var rootPath = previousPath.Take(spurIndex + 1).ToList();

                    // Block edges that have been used by paths with the same root
                    var blockedEdges = new HashSet<(MazePoint, MazePoint)>();

                    foreach (var existingResult in results)
                    {
                        var existingPathNodes = GetPathNodes(existingResult, startPoint);
                        if (existingPathNodes.Count > spurIndex &&
                            PathsShareRoot(rootPath, existingPathNodes.Take(spurIndex + 1).ToList()))
                        {
                            // Block the edge from spur node to next node in existing path
                            if (existingPathNodes.Count > spurIndex + 1)
                            {
                                blockedEdges.Add((existingPathNodes[spurIndex], existingPathNodes[spurIndex + 1]));
                            }
                        }
                    }

                    // Block edges to nodes in root path (except the spur node)
                    foreach (var node in rootPath.Take(spurIndex))
                    {
                        foreach (var targetNode in graph.Nodes.Keys)
                        {
                            blockedEdges.Add((targetNode, node));
                            blockedEdges.Add((node, targetNode));
                        }
                    }

                    // Find shortest path from spur node to end with blocked edges
                    var spurPath = ComputeShortestPath(graph, spurNode, endPoint, blockedEdges);

                    if (spurPath != null)
                    {
                        // Combine root path directions with spur path
                        var totalPath = new List<Direction>();

                        // Get directions for root path
                        if (spurIndex > 0)
                        {
                            totalPath.AddRange(GetDirectionsBetweenNodes(graph, rootPath));
                        }

                        // Add spur path directions
                        totalPath.AddRange(spurPath);

                        var candidate = new CandidatePath
                        {
                            Directions = totalPath,
                            Distance = totalPath.Count
                        };

                        // Only add if this path is unique
                        if (!candidates.Any(c => DirectionsEqual(c.Directions, totalPath)) &&
                            !results.Any(r => DirectionsEqual(r.Directions, totalPath)))
                        {
                            candidates.Add(candidate);
                        }
                    }
                }

                // Take the best candidate as the next path
                if (candidates.Count > 0)
                {
                    var best = candidates.First();
                    candidates.Remove(best);
                    var optimalDistance = results[0].TotalDistance;
                    var pathResult = CreatePathResult(best.Directions, graph, startPoint, pathIndex, optimalDistance);
                    results.Add(pathResult);
                }
                else
                {
                    // No more alternative paths found
                    break;
                }
            }

            return results;
        }

        /// <summary>
        /// Computes the shortest path using Dijkstra's algorithm with local state.
        /// Does NOT mutate the graph - uses local dictionaries for distances and path directions.
        /// </summary>
        private List<Direction>? ComputeShortestPath(
            Graph graph,
            MazePoint startPoint,
            MazePoint endPoint,
            HashSet<(MazePoint, MazePoint)> blockedEdges)
        {
            if (!graph.Nodes.ContainsKey(startPoint) || !graph.Nodes.ContainsKey(endPoint))
            {
                return null;
            }

            // Use local dictionaries instead of mutating graph nodes
            var distances = new Dictionary<MazePoint, int>();
            var pathDirections = new Dictionary<MazePoint, Direction[]>();

            // Initialize all nodes with infinity distance
            foreach (var point in graph.Nodes.Keys)
            {
                distances[point] = int.MaxValue;
                pathDirections[point] = [];
            }

            distances[startPoint] = 0;

            var queue = new List<MazePoint>(graph.Nodes.Keys);

            while (queue.Count > 0)
            {
                // Get node with smallest distance
                MazePoint? currentPoint = null;
                int minDist = int.MaxValue;

                foreach (var point in queue)
                {
                    if (distances[point] < minDist)
                    {
                        minDist = distances[point];
                        currentPoint = point;
                    }
                }

                if (currentPoint == null || minDist == int.MaxValue)
                {
                    break;
                }

                queue.Remove(currentPoint);

                if (currentPoint.Equals(endPoint))
                {
                    break;
                }

                var currentNode = graph.Nodes[currentPoint];

                foreach (var edge in currentNode.Edges)
                {
                    // Skip blocked edges
                    if (blockedEdges.Contains((currentPoint, edge.Point)))
                    {
                        continue;
                    }

                    if (!graph.Nodes.ContainsKey(edge.Point))
                    {
                        continue;
                    }

                    var newDistance = distances[currentPoint] + edge.DirectionsToPoint.Length;

                    if (newDistance < distances[edge.Point])
                    {
                        distances[edge.Point] = newDistance;
                        pathDirections[edge.Point] = [.. pathDirections[currentPoint], .. edge.DirectionsToPoint];
                    }
                }
            }

            if (distances[endPoint] == int.MaxValue)
            {
                return null;
            }

            return pathDirections[endPoint].ToList();
        }

        private bool HasAlternativePaths(Graph graph)
        {
            // A maze has alternative paths if any node has more than 2 edges
            // (indicating a junction where the path could diverge and rejoin)
            // or if any node has edges that form a cycle
            // For simplicity, check if total edges > nodes - 1 (tree structure)
            int totalEdges = 0;
            foreach (var node in graph.Nodes.Values)
            {
                totalEdges += node.Edges.Length;
            }

            // Each edge is counted twice (once from each direction)
            // A tree has exactly N-1 edges for N nodes
            int edgeCount = totalEdges / 2;
            return edgeCount > graph.Nodes.Count - 1;
        }

        private List<MazePoint> GetPathNodes(PathResult pathResult, MazePoint startPoint)
        {
            var nodes = new List<MazePoint> { startPoint };
            nodes.AddRange(pathResult.JunctionNodes.Skip(1)); // Skip start if it's in JunctionNodes
            return nodes.Distinct().ToList();
        }

        private bool PathsShareRoot(List<MazePoint> path1, List<MazePoint> path2)
        {
            if (path1.Count != path2.Count) return false;

            for (int i = 0; i < path1.Count; i++)
            {
                if (!path1[i].Equals(path2[i])) return false;
            }

            return true;
        }

        private List<Direction> GetDirectionsBetweenNodes(Graph graph, List<MazePoint> nodes)
        {
            var directions = new List<Direction>();

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var currentNode = graph.Nodes[nodes[i]];
                var nextPoint = nodes[i + 1];

                var edge = currentNode.Edges.FirstOrDefault(e => e.Point.Equals(nextPoint));
                if (edge != null)
                {
                    directions.AddRange(edge.DirectionsToPoint);
                }
            }

            return directions;
        }

        private bool DirectionsEqual(List<Direction> a, List<Direction> b)
        {
            if (a.Count != b.Count) return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }

            return true;
        }

        private PathResult CreatePathResult(List<Direction> directions, Graph graph, MazePoint startPoint, int pathIndex, int optimalDistance)
        {
            var pathResult = new PathResult
            {
                PathIndex = pathIndex,
                TotalDistance = directions.Count,
                DistanceFromOptimal = directions.Count - optimalDistance,
                Directions = directions
            };

            // Build junction nodes and path edges
            var currentPoint = startPoint;
            MazePoint? lastJunction = graph.Nodes.ContainsKey(startPoint) ? startPoint : null;

            if (lastJunction != null)
            {
                pathResult.JunctionNodes.Add(startPoint);
            }

            foreach (var direction in directions)
            {
                currentPoint = MovePoint(currentPoint, direction);

                if (graph.Nodes.ContainsKey(currentPoint))
                {
                    pathResult.JunctionNodes.Add(currentPoint);

                    if (lastJunction != null)
                    {
                        pathResult.PathEdges.Add((lastJunction, currentPoint));
                    }
                    lastJunction = currentPoint;
                }
            }

            return pathResult;
        }

        private static MazePoint MovePoint(MazePoint point, Direction direction)
        {
            return direction switch
            {
                Direction.Left => new MazePoint(point.X - 1, point.Y, point.Z),
                Direction.Right => new MazePoint(point.X + 1, point.Y, point.Z),
                Direction.Forward => new MazePoint(point.X, point.Y + 1, point.Z),
                Direction.Back => new MazePoint(point.X, point.Y - 1, point.Z),
                Direction.Up => new MazePoint(point.X, point.Y, point.Z + 1),
                Direction.Down => new MazePoint(point.X, point.Y, point.Z - 1),
                _ => point
            };
        }

        private class CandidatePath
        {
            public List<Direction> Directions { get; set; } = new();
            public int Distance { get; set; }
        }

        private class CandidatePathComparer : IComparer<CandidatePath>
        {
            public int Compare(CandidatePath? x, CandidatePath? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                int distCompare = x.Distance.CompareTo(y.Distance);
                if (distCompare != 0) return distCompare;

                // Use hash code as tiebreaker to allow duplicates with same distance
                return x.GetHashCode().CompareTo(y.GetHashCode());
            }
        }
    }
}

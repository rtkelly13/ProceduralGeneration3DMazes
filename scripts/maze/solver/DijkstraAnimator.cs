using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    /// <summary>
    /// Captures the step-by-step execution of Dijkstra's algorithm for animation.
    /// </summary>
    public class DijkstraAnimator
    {
        private readonly IGraphBuilder _graphBuilder;

        public DijkstraAnimator(IGraphBuilder graphBuilder)
        {
            _graphBuilder = graphBuilder;
        }

        /// <summary>
        /// Runs Dijkstra's algorithm and captures each step for animation.
        /// </summary>
        public List<AlgorithmStep> CaptureSteps(IMazeJumper jumper)
        {
            var graph = _graphBuilder.GetGraphFromMaze(jumper);
            return CaptureSteps(graph, jumper.StartPoint, jumper.EndPoint);
        }

        /// <summary>
        /// Runs Dijkstra's algorithm on an existing graph and captures each step.
        /// </summary>
        public List<AlgorithmStep> CaptureSteps(Graph graph, MazePoint startPoint, MazePoint endPoint)
        {
            var steps = new List<AlgorithmStep>();
            
            // Initialize all distances
            var distances = new Dictionary<MazePoint, int>();
            var predecessors = new Dictionary<MazePoint, MazePoint>();
            var visited = new HashSet<MazePoint>();
            var frontier = new HashSet<MazePoint>();

            foreach (var point in graph.Nodes.Keys)
            {
                distances[point] = int.MaxValue;
            }
            distances[startPoint] = 0;
            frontier.Add(startPoint);

            // Record initialization step
            steps.Add(new AlgorithmStep
            {
                Type = StepType.Initialize,
                Description = $"Initialize: Set distance to start ({startPoint.X},{startPoint.Y}) = 0, all others = infinity",
                CurrentNode = startPoint,
                VisitedNodes = new HashSet<MazePoint>(visited),
                FrontierNodes = new HashSet<MazePoint>(frontier),
                Distances = new Dictionary<MazePoint, int>(distances),
                Predecessors = new Dictionary<MazePoint, MazePoint>(predecessors)
            });

            var queue = new List<MazePoint>(graph.Nodes.Keys);

            while (queue.Count > 0)
            {
                // Find node with minimum distance
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
                frontier.Remove(currentPoint);
                visited.Add(currentPoint);

                // Record node selection step
                steps.Add(new AlgorithmStep
                {
                    Type = StepType.SelectNode,
                    Description = $"Select node ({currentPoint.X},{currentPoint.Y}) with distance {minDist}",
                    CurrentNode = currentPoint,
                    VisitedNodes = new HashSet<MazePoint>(visited),
                    FrontierNodes = new HashSet<MazePoint>(frontier),
                    Distances = new Dictionary<MazePoint, int>(distances),
                    Predecessors = new Dictionary<MazePoint, MazePoint>(predecessors)
                });

                // If we've reached the end, we can stop
                if (currentPoint.Equals(endPoint))
                {
                    break;
                }

                var currentNode = graph.Nodes[currentPoint];

                // Examine each edge
                foreach (var edge in currentNode.Edges)
                {
                    if (!graph.Nodes.ContainsKey(edge.Point) || visited.Contains(edge.Point))
                    {
                        continue;
                    }

                    int edgeWeight = edge.DirectionsToPoint.Length;
                    int newDistance = distances[currentPoint] + edgeWeight;
                    int oldDistance = distances[edge.Point];

                    // Record edge examination
                    steps.Add(new AlgorithmStep
                    {
                        Type = StepType.ExamineEdge,
                        Description = $"Examine edge to ({edge.Point.X},{edge.Point.Y}): current {oldDistance}, via this edge {newDistance}",
                        CurrentNode = currentPoint,
                        NeighborNode = edge.Point,
                        CurrentEdge = (currentPoint, edge.Point),
                        OldDistance = oldDistance == int.MaxValue ? null : oldDistance,
                        NewDistance = newDistance,
                        VisitedNodes = new HashSet<MazePoint>(visited),
                        FrontierNodes = new HashSet<MazePoint>(frontier),
                        Distances = new Dictionary<MazePoint, int>(distances),
                        Predecessors = new Dictionary<MazePoint, MazePoint>(predecessors)
                    });

                    if (newDistance < oldDistance)
                    {
                        distances[edge.Point] = newDistance;
                        predecessors[edge.Point] = currentPoint;
                        
                        if (!frontier.Contains(edge.Point))
                        {
                            frontier.Add(edge.Point);
                        }

                        // Record relaxation
                        steps.Add(new AlgorithmStep
                        {
                            Type = StepType.RelaxEdge,
                            Description = $"Relax: Update ({edge.Point.X},{edge.Point.Y}) distance {(oldDistance == int.MaxValue ? "inf" : oldDistance.ToString())} -> {newDistance}",
                            CurrentNode = currentPoint,
                            NeighborNode = edge.Point,
                            CurrentEdge = (currentPoint, edge.Point),
                            OldDistance = oldDistance == int.MaxValue ? null : oldDistance,
                            NewDistance = newDistance,
                            VisitedNodes = new HashSet<MazePoint>(visited),
                            FrontierNodes = new HashSet<MazePoint>(frontier),
                            Distances = new Dictionary<MazePoint, int>(distances),
                            Predecessors = new Dictionary<MazePoint, MazePoint>(predecessors)
                        });
                    }
                    else
                    {
                        // Record skip
                        steps.Add(new AlgorithmStep
                        {
                            Type = StepType.SkipEdge,
                            Description = $"Skip: ({edge.Point.X},{edge.Point.Y}) already has shorter path ({oldDistance} <= {newDistance})",
                            CurrentNode = currentPoint,
                            NeighborNode = edge.Point,
                            CurrentEdge = (currentPoint, edge.Point),
                            OldDistance = oldDistance,
                            NewDistance = newDistance,
                            VisitedNodes = new HashSet<MazePoint>(visited),
                            FrontierNodes = new HashSet<MazePoint>(frontier),
                            Distances = new Dictionary<MazePoint, int>(distances),
                            Predecessors = new Dictionary<MazePoint, MazePoint>(predecessors)
                        });
                    }
                }

                // Record node finalization
                steps.Add(new AlgorithmStep
                {
                    Type = StepType.FinalizeNode,
                    Description = $"Finalize node ({currentPoint.X},{currentPoint.Y})",
                    CurrentNode = currentPoint,
                    VisitedNodes = new HashSet<MazePoint>(visited),
                    FrontierNodes = new HashSet<MazePoint>(frontier),
                    Distances = new Dictionary<MazePoint, int>(distances),
                    Predecessors = new Dictionary<MazePoint, MazePoint>(predecessors)
                });
            }

            // Record completion
            steps.Add(new AlgorithmStep
            {
                Type = StepType.Complete,
                Description = $"Complete: Shortest path to ({endPoint.X},{endPoint.Y}) = {distances.GetValueOrDefault(endPoint, int.MaxValue)}",
                VisitedNodes = new HashSet<MazePoint>(visited),
                FrontierNodes = new HashSet<MazePoint>(frontier),
                Distances = new Dictionary<MazePoint, int>(distances),
                Predecessors = new Dictionary<MazePoint, MazePoint>(predecessors)
            });

            return steps;
        }
    }
}

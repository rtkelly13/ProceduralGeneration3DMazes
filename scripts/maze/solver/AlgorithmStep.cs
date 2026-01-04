using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Solver
{
    /// <summary>
    /// Type of algorithm step during Dijkstra animation.
    /// </summary>
    public enum StepType
    {
        /// <summary>
        /// Initial state before algorithm starts.
        /// </summary>
        Initialize,

        /// <summary>
        /// A node is being selected from the priority queue.
        /// </summary>
        SelectNode,

        /// <summary>
        /// An edge is being examined for potential relaxation.
        /// </summary>
        ExamineEdge,

        /// <summary>
        /// An edge was relaxed (distance improved).
        /// </summary>
        RelaxEdge,

        /// <summary>
        /// An edge was not relaxed (existing path is shorter).
        /// </summary>
        SkipEdge,

        /// <summary>
        /// A node has been fully processed.
        /// </summary>
        FinalizeNode,

        /// <summary>
        /// Algorithm has completed.
        /// </summary>
        Complete
    }

    /// <summary>
    /// Represents a single step in the Dijkstra algorithm execution.
    /// </summary>
    public class AlgorithmStep
    {
        /// <summary>
        /// Type of this step.
        /// </summary>
        public StepType Type { get; set; }

        /// <summary>
        /// Human-readable description of what's happening.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The current node being processed (if applicable).
        /// </summary>
        public MazePoint? CurrentNode { get; set; }

        /// <summary>
        /// The neighbor node being examined (if applicable).
        /// </summary>
        public MazePoint? NeighborNode { get; set; }

        /// <summary>
        /// Edge being examined or relaxed (from, to).
        /// </summary>
        public (MazePoint From, MazePoint To)? CurrentEdge { get; set; }

        /// <summary>
        /// Set of nodes that have been fully processed.
        /// </summary>
        public HashSet<MazePoint> VisitedNodes { get; set; } = new();

        /// <summary>
        /// Set of nodes in the frontier (in priority queue with finite distance).
        /// </summary>
        public HashSet<MazePoint> FrontierNodes { get; set; } = new();

        /// <summary>
        /// Current distances from start to each node.
        /// </summary>
        public Dictionary<MazePoint, int> Distances { get; set; } = new();

        /// <summary>
        /// Current predecessors for path reconstruction.
        /// </summary>
        public Dictionary<MazePoint, MazePoint> Predecessors { get; set; } = new();

        /// <summary>
        /// Old distance before relaxation (for RelaxEdge steps).
        /// </summary>
        public int? OldDistance { get; set; }

        /// <summary>
        /// New distance after relaxation (for RelaxEdge steps).
        /// </summary>
        public int? NewDistance { get; set; }

        /// <summary>
        /// Creates a deep copy of this step for storage.
        /// </summary>
        public AlgorithmStep Clone()
        {
            return new AlgorithmStep
            {
                Type = Type,
                Description = Description,
                CurrentNode = CurrentNode,
                NeighborNode = NeighborNode,
                CurrentEdge = CurrentEdge,
                VisitedNodes = new HashSet<MazePoint>(VisitedNodes),
                FrontierNodes = new HashSet<MazePoint>(FrontierNodes),
                Distances = new Dictionary<MazePoint, int>(Distances),
                Predecessors = new Dictionary<MazePoint, MazePoint>(Predecessors),
                OldDistance = OldDistance,
                NewDistance = NewDistance
            };
        }
    }
}

using System.Collections.Generic;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Agents
{
    /// <summary>
    /// Walks the maze to the end point, reporting the route it took.
    /// </summary>
    /// <remarks>
    /// Implemented as an **iterative depth-first search with a shared visited set**. Both
    /// details are load-bearing; the previous version had neither and was pathologically slow.
    ///
    /// WHAT WAS WRONG
    ///
    /// The old search tracked visited cells *per path* — <c>previousPoints.Any(x => …)</c>, a
    /// linear scan of the current route — rather than once for the whole search. A cell reachable
    /// by several routes was therefore re-explored once per route, making the search exponential
    /// in the worst case. It also rebuilt the entire path per branch
    /// (<c>previousPoints.Concat(…).ToList()</c>), adding an O(path) allocation per step.
    ///
    /// Measured on the 1200- and 1600-cell sample mazes, 8 runs of two tests: 1.8s, 1.9s, 2.0s,
    /// 3.2s, 14.6s, 25.9s, and two runs still unfinished at 120s. The same runs with RandomAgent
    /// were flat at 1.6–1.8s. That tail is what made CI wall-time range from 29s to a 40-minute
    /// hang on identical code.
    ///
    /// WHY ITERATIVE
    ///
    /// Recursion depth tracked path length, so a large maze risked a stack overflow independently
    /// of the exponential blowup — the app allows up to 50 cells per axis, i.e. tens of thousands
    /// of cells. An explicit stack removes that ceiling.
    ///
    /// WHAT IS UNCHANGED
    ///
    /// Still "the first route DFS finds", not a guaranteed shortest path. In a perfect maze
    /// (a spanning tree) exactly one simple route exists between any two cells, so that route is
    /// the shortest — which is what the existing tests assert. With wall removal the maze gains
    /// loops and DFS may return a longer route; that was equally true before. Direction order is
    /// still shuffled through the injected generator, so a seed still reproduces the walk.
    /// </remarks>
    public class PerfectAgent : AgentBase
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IRandomValueGenerator _randomValueGenerator;

        public PerfectAgent(IDirectionsFlagParser directionsFlagParser,
            IRandomValueGenerator randomValueGenerator)
        {
            _directionsFlagParser = directionsFlagParser;
            _randomValueGenerator = randomValueGenerator;
        }

        /// <summary>One search frame: the cell's directions and how many have been tried.</summary>
        private sealed class Frame
        {
            public Direction[] Directions = [];
            public int Next;
        }

        public override AgentResults RunAgentBase(IMaze maze)
        {
            var movements = new List<DirectionAndPoint>();

            // Start == end happens on a single-cell maze; there is nothing to walk.
            if (maze.CurrentPoint.Equals(maze.EndPoint))
            {
                return new AgentResults { Movements = movements };
            }

            // Shared across the whole search, not per path. This is the fix: a cell is
            // explored at most once, making the walk linear in the number of cells.
            var visited = new HashSet<MazePoint> { maze.CurrentPoint };
            var stack = new Stack<Frame>();
            stack.Push(NewFrame(maze));

            while (stack.Count > 0)
            {
                var frame = stack.Peek();

                if (frame.Next >= frame.Directions.Length)
                {
                    // Every direction from this cell is exhausted: back out of it.
                    stack.Pop();
                    if (stack.Count > 0)
                    {
                        // Undo the move that led here. The root frame has no such move, which is
                        // why this is guarded — reaching it means the end is unreachable.
                        var retreat = movements[^1];
                        movements.RemoveAt(movements.Count - 1);
                        maze.MoveInDirection(_directionsFlagParser.OppositeDirection(retreat.Direction));
                    }

                    continue;
                }

                var direction = frame.Directions[frame.Next++];
                var from = maze.CurrentPoint;
                maze.MoveInDirection(direction);

                if (!visited.Add(maze.CurrentPoint))
                {
                    // Already explored from another route; step back and try the next direction.
                    maze.MoveInDirection(_directionsFlagParser.OppositeDirection(direction));
                    continue;
                }

                movements.Add(new DirectionAndPoint { Direction = direction, MazePoint = from });

                if (maze.CurrentPoint.Equals(maze.EndPoint))
                {
                    return new AgentResults { Movements = movements };
                }

                stack.Push(NewFrame(maze));
            }

            // No route exists. Returns an empty walk rather than a partial one, so a caller
            // cannot mistake a dead search for a real route.
            return new AgentResults { Movements = [] };
        }

        private Frame NewFrame(IMaze maze)
        {
            var directions = maze.GetDirectionsFromPoint();
            _randomValueGenerator.Shuffle(directions);
            return new Frame { Directions = directions };
        }
    }
}

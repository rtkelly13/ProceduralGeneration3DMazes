using System;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Helper;

namespace ProceduralMaze.Maze.Model
{
    /// <summary>
    /// Array-based maze model that stores connections unidirectionally.
    /// Each edge is stored only once (in the cell with the "positive" direction: Right, Up, Forward).
    /// Uses less memory but requires checking adjacent cells for Left/Down/Back directions.
    /// </summary>
    public class ArrayUnidirectionalModel : ModelBase
    {
        private readonly IDirectionsFlagParser _flagParser;
        private readonly IMovementHelper _movementHelper;
        private readonly IMazeArrayBuilder _mazeArrayBuilder;

        private MazeCell[,,] Maze { get; set; } = null!;

        public ArrayUnidirectionalModel(IDirectionsFlagParser flagParser, IMovementHelper movementHelper, IMazeArrayBuilder mazeArrayBuilder)
        {
            _flagParser = flagParser;
            _movementHelper = movementHelper;
            _mazeArrayBuilder = mazeArrayBuilder;
        }

        protected override void Initialise(ModelInitialisationOptions options)
        {
            Maze = _mazeArrayBuilder.Build(options.Size);
        }

        public override void PlaceVertex(MazePoint p, Direction d)
        {
            if (_movementHelper.CanMove(p, d, Size, out MazePoint finalPoint))
            {
                MazeCell cell;
                switch (d)
                {
                    case Direction.Right:
                    case Direction.Up:
                    case Direction.Forward:
                        cell = Maze[p.X, p.Y, p.Z];
                        cell.Directions = _flagParser.AddDirectionsToFlag(cell.Directions, d);
                        break;
                    case Direction.Left:
                    case Direction.Down:
                    case Direction.Back:
                        cell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                        cell.Directions = _flagParser.AddDirectionsToFlag(cell.Directions,
                            _flagParser.OppositeDirection(d));
                        break;
                    default:
                        throw new ArgumentException($"Flag not supported: {d}");
                }
            }
        }

        public override void RemoveVertex(MazePoint p, Direction d)
        {
            if (_movementHelper.CanMove(p, d, Size, out MazePoint finalPoint))
            {
                MazeCell cell;
                if (IsRightUpForward(d))
                {
                    cell = Maze[p.X, p.Y, p.Z];
                    cell.Directions = _flagParser.RemoveDirectionsFromFlag(cell.Directions, d);
                }
                else if (IsLeftDownBack(d))
                {
                    cell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                    cell.Directions = _flagParser.RemoveDirectionsFromFlag(cell.Directions,
                        _flagParser.OppositeDirection(d));
                }
                else
                {
                    throw new ArgumentException($"Flag not supported: {d}");
                }
            }
        }

        public override bool HasDirections(MazePoint p, Direction d)
        {
            var directions = _flagParser.SplitDirectionsFromFlag(d);
            foreach (var x in directions)
            {
                if (IsRightUpForward(x))
                {
                    if (!_flagParser.FlagHasDirections(Maze[p.X, p.Y, p.Z].Directions, x))
                        return false;
                }
                else if (IsLeftDownBack(x))
                {
                    if (!_flagParser.FlagHasDirections(HasVertexInDirection(p, x), x))
                        return false;
                }
                else
                {
                    throw new ArgumentException($"Flag not supported: {d}");
                }
            }
            return true;
        }

        public override Direction GetFlagFromPoint(MazePoint p)
        {
            var currentCell = Maze[p.X, p.Y, p.Z].Directions & (Direction.Right | Direction.Up | Direction.Forward);
            return currentCell |
                   HasVertexInDirection(p, Direction.Left) |
                   HasVertexInDirection(p, Direction.Down) |
                   HasVertexInDirection(p, Direction.Back);
        }

        private Direction HasVertexInDirection(MazePoint p, Direction d)
        {
            var canMove = _movementHelper.CanMove(p, d, Size, out MazePoint finalPoint);
            if (canMove)
            {
                // Check if the adjacent cell has a connection back to us (stored as the opposite direction)
                // If so, return the original direction d (not the opposite)
                var oppositeDir = _flagParser.OppositeDirection(d);
                if (_flagParser.FlagHasDirections(Maze[finalPoint.X, finalPoint.Y, finalPoint.Z].Directions, oppositeDir))
                {
                    return d;
                }
            }
            return Direction.None;
        }

        private bool IsRightUpForward(Direction x)
        {
            return x == Direction.Right || x == Direction.Up || x == Direction.Forward;
        }

        private bool IsLeftDownBack(Direction x)
        {
            return x == Direction.Left || x == Direction.Down || x == Direction.Back;
        }
    }
}

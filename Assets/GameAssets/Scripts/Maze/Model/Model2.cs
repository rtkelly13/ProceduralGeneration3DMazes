using System;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class Model2 : ModelBase
    {
        private readonly IDirectionsFlagParser _flagParser;
        private readonly IMovementHelper _movementHelper;
        private readonly IMazeArrayBuilder _mazeArrayBuilder;

        private MazeCell[,,] Maze { get; set; }

        public Model2(IDirectionsFlagParser flagParser, IMovementHelper movementHelper, IMazeArrayBuilder mazeArrayBuilder)
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
            MazePoint finalPoint;
            if (_movementHelper.CanMove(p, d, Size, out finalPoint))
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
                        throw new ArgumentException(String.Format("Flag not supported: {0}", d));
                }
            }
        }

        public override void RemoveVertex(MazePoint p, Direction d)
        {
            MazePoint finalPoint;
            if (_movementHelper.CanMove(p, d, Size, out finalPoint))
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
                    throw new ArgumentException(String.Format("Flag not supported: {0}", d));
                }
            }
        }

        public override bool HasDirections(MazePoint p, Direction d)
        {
            var directions = _flagParser.SplitDirectionsFromFlag(d);
            return directions.All(x =>
            {
                if (IsRightUpForward(x))
                {
                    return _flagParser.FlagHasDirections(Maze[p.X, p.Y, p.Z].Directions, x);
                }
                if (IsLeftDownBack(x))
                {
                    return _flagParser.FlagHasDirections(HasVertexInDirection(p, x), x);
                }
                throw new ArgumentException(String.Format("Flag not supported: {0}", d));
            });
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
            MazePoint finalPoint;
            var canMove = _movementHelper.CanMove(p, d, Size, out finalPoint);
            if (canMove)
            {
                return Maze[finalPoint.X, finalPoint.Y, finalPoint.Z].Directions & _flagParser.OppositeDirection(d);
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

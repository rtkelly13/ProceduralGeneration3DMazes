using System.Linq;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class MazeModel2 : MazeBase, IBuilder
    {
        private readonly IMazePointFactory _pointFactory;
        private readonly IMovementHelper _movementHelper;

        private MazeCell[,,] Maze { get; set; }

        public MazeModel2(IDirectionsFlagParser flagParser, IMazePointFactory pointFactory, IMovementHelper movementHelper) : base(flagParser)
        {
            _pointFactory = pointFactory;
            _movementHelper = movementHelper;
        }


        protected override void Initialise(MazeSize size, bool allVertexes)
        {
            Size = size;
            Maze = new MazeCell[size.X, size.Z, size.Y];
            var xValues = Enumerable.Range(0, size.X).ToList();
            var yValues = Enumerable.Range(0, size.Z).ToList();
            var zValues = Enumerable.Range(0, size.Y).ToList();
            foreach (var point in xValues.SelectMany(x => yValues.SelectMany(y => zValues.Select(z => _pointFactory.MakePoint(x, y, z)))))
            {
                Maze[point.X, point.Y, point.Z] = new MazeCell
                {
                    Directions = Direction.None
                };
            }
        }

        public void PlaceVertex(MazePoint p, Direction d)
        {
            MazePoint finalPoint;
            if (_movementHelper.CanMove(p, d, Size, out finalPoint))
            {
                MazeCell cell;
                switch (d)
                {
                    case Direction.Left:
                    case Direction.Up:
                    case Direction.Forward:
                        cell = Maze[p.X, p.Y, p.Z];
                        cell.Directions = FlagParser.AddDirectionsToFlag(cell.Directions, d);
                        break;
                    case Direction.Right:
                    case Direction.Down:
                    case Direction.Back:
                        cell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                        cell.Directions = FlagParser.AddDirectionsToFlag(cell.Directions,
                        FlagParser.OppositeDirection(d));
                        break;
                }
            }
        }

        public void RemoveVertex(MazePoint p, Direction d)
        {
            MazePoint finalPoint;
            if (_movementHelper.CanMove(p, d, Size, out finalPoint))
            {
                MazeCell cell;
                switch (d)
                {
                    case Direction.Left:
                    case Direction.Up:
                    case Direction.Forward:
                        cell = Maze[p.X, p.Y, p.Z];
                        cell.Directions = FlagParser.RemoveDirectionsFromFlag(cell.Directions, d);
                        break;
                    case Direction.Right:
                    case Direction.Down:
                    case Direction.Back:
                        cell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                        cell.Directions = FlagParser.RemoveDirectionsFromFlag(cell.Directions,
                        FlagParser.OppositeDirection(d));
                        break;
                }
            }
        }

        public override Direction GetFlagFromPoint(MazePoint p)
        {
            var currentCell = Maze[p.X, p.Y, p.Z].Directions & (Direction.Left | Direction.Up | Direction.Forward);
            return currentCell |
                   HasVertexInDirection(p, Direction.Right) |
                   HasVertexInDirection(p, Direction.Down) |
                   HasVertexInDirection(p, Direction.Back);
        }

        private Direction HasVertexInDirection(MazePoint p, Direction d)
        {
            var finalPoint = _movementHelper.Move(p, Direction.Right, Size);
            return Maze[finalPoint.X, finalPoint.Y, finalPoint.Z].Directions & FlagParser.OppositeDirection(d);
        }
    }
}

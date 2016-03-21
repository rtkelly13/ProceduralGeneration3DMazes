using System.Linq;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class MazeModel1 : MazeBase, IBuilder
    {
        private readonly IMazePointFactory _pointFactory;
        private readonly IMovementHelper _movementHelper;

        private MazeCell[,,] Maze { get; set; }

        public MazeModel1(IDirectionsFlagParser flagParser, IMazePointFactory pointFactory, IMovementHelper movementHelper) : base(flagParser)
        {
            _pointFactory = pointFactory;
            _movementHelper = movementHelper;
        }

        protected override void Initialise(MazeSize size)
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

        public override void PlaceVertex(MazePoint p, Direction d)
        {
            var startCell = Maze[p.X, p.Y, p.Z];
            MazePoint finalPoint;
            if (_movementHelper.CanMove(p, d, Size, out finalPoint))
            {
                var finalCell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                startCell.Directions = FlagParser.AddDirectionsToFlag(startCell.Directions, d);
                finalCell.Directions = FlagParser.AddDirectionsToFlag(finalCell.Directions,
                    FlagParser.OppositeDirection(d));
            }
        }

        public override void RemoveVertex(MazePoint p, Direction d)
        {
            var startCell = Maze[p.X, p.Y, p.Z];
            MazePoint finalPoint;
            if (_movementHelper.CanMove(p, d, Size, out finalPoint))
            {
                var finalCell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                startCell.Directions = FlagParser.RemoveDirectionsFromFlag(startCell.Directions, d);
                finalCell.Directions = FlagParser.RemoveDirectionsFromFlag(finalCell.Directions,
                    FlagParser.OppositeDirection(d));
            }
        }

        public override bool HasDirections(MazePoint p, Direction d)
        {
            return FlagParser.FlagHasDirections(GetFlagFromPoint(p), d);
        }

        public override Direction GetFlagFromPoint(MazePoint p)
        {
            return Maze[p.X, p.Y, p.Z].Directions;
        }
    }
}

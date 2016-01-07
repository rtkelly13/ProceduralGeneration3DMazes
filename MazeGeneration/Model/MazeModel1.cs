using System.Linq;
using MazeGeneration.Factory;
using MazeGeneration.Helper;

namespace MazeGeneration.Model
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

        protected override void Initialise(MazeSize size, bool allVertexes)
        {
            Size = size;
            Maze = new MazeCell[size.Width, size.Height, size.Depth];
            var width = Enumerable.Range(0, size.Width).ToList();
            var height = Enumerable.Range(0, size.Height).ToList();
            var depth = Enumerable.Range(0, size.Depth).ToList();
            foreach (var point in width.SelectMany(x => height.SelectMany(y => depth.Select(z => _pointFactory.MakePoint(x, y, z)))))
            {
                Maze[point.X, point.Y, point.Z] = new MazeCell
                {
                    Directions = Direction.None
                };
            }
        }

        public void PlaceVertex(MazePoint p, Direction d)
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

        public void RemoveVertex(MazePoint p, Direction d)
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

        public override Direction GetFlagFromPoint(MazePoint p)
        {
            return Maze[p.X, p.Y, p.Z].Directions;
        }
    }
}

using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Helper;

namespace ProceduralMaze.Maze.Model
{
    /// <summary>
    /// Array-based maze model that stores connections bidirectionally.
    /// Each edge is stored in both cells (e.g., cell A stores "Right" and cell B stores "Left").
    /// Uses more memory but provides O(1) lookup for all directions from any cell.
    /// </summary>
    public class ArrayBidirectionalModel : ModelBase
    {
        private readonly IDirectionsFlagParser _flagParser;
        private readonly IMovementHelper _movementHelper;
        private readonly IMazeArrayBuilder _mazeArrayBuilder;

        private MazeCell[,,] Maze { get; set; } = null!;

        public ArrayBidirectionalModel(IDirectionsFlagParser flagParser, IMovementHelper movementHelper, IMazeArrayBuilder mazeArrayBuilder)
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
            var startCell = Maze[p.X, p.Y, p.Z];
            if (_movementHelper.CanMove(p, d, Size, out var finalPoint))
            {
                var finalCell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                startCell.Directions = _flagParser.AddDirectionsToFlag(startCell.Directions, d);
                finalCell.Directions = _flagParser.AddDirectionsToFlag(finalCell.Directions,
                    _flagParser.OppositeDirection(d));
            }
        }

        public override void RemoveVertex(MazePoint p, Direction d)
        {
            var startCell = Maze[p.X, p.Y, p.Z];
            if (_movementHelper.CanMove(p, d, Size, out var finalPoint))
            {
                var finalCell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                startCell.Directions = _flagParser.RemoveDirectionsFromFlag(startCell.Directions, d);
                finalCell.Directions = _flagParser.RemoveDirectionsFromFlag(finalCell.Directions,
                    _flagParser.OppositeDirection(d));
            }
        }

        public override bool HasDirections(MazePoint p, Direction d)
        {
            return _flagParser.FlagHasDirections(GetFlagFromPoint(p), d);
        }

        public override Direction GetFlagFromPoint(MazePoint p)
        {
            return Maze[p.X, p.Y, p.Z].Directions;
        }
    }
}

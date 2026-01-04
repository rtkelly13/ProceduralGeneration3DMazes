using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Helper;

namespace ProceduralMaze.Maze.Model
{
    /// <summary>
    /// Dictionary-based maze model that stores connections bidirectionally.
    /// Uses a Dictionary instead of a 3D array for cell storage.
    /// Each edge is stored in both cells. Better for sparse mazes or dynamic sizing.
    /// </summary>
    public class DictionaryModel : ModelBase
    {
        private readonly IDirectionsFlagParser _flagParser;
        private readonly IMazePointFactory _pointFactory;
        private readonly IMovementHelper _movementHelper;

        private Dictionary<MazePoint, MazeCell> Maze { get; set; } = null!;

        public DictionaryModel(IDirectionsFlagParser flagParser, IMazePointFactory pointFactory, IMovementHelper movementHelper)
        {
            _flagParser = flagParser;
            _pointFactory = pointFactory;
            _movementHelper = movementHelper;
        }

        protected override void Initialise(ModelInitialisationOptions options)
        {
            Maze = new Dictionary<MazePoint, MazeCell>();
            var size = options.Size;
            var xValues = Enumerable.Range(0, size.X).ToList();
            var yValues = Enumerable.Range(0, size.Y).ToList();
            var zValues = Enumerable.Range(0, size.Z).ToList();
            foreach (var point in xValues.SelectMany(x => yValues.SelectMany(y => zValues.Select(z => _pointFactory.MakePoint(x, y, z)))))
            {
                Maze.Add(point, new MazeCell
                {
                    Directions = Direction.None
                });
            }
        }

        public override void PlaceVertex(MazePoint p, Direction d)
        {
            if (Maze.TryGetValue(p, out MazeCell? startCell) && _movementHelper.CanMove(p, d, Size, out MazePoint final) && Maze.TryGetValue(final, out MazeCell? finalCell))
            {
                startCell.Directions = _flagParser.AddDirectionsToFlag(startCell.Directions, d);
                finalCell.Directions = _flagParser.AddDirectionsToFlag(finalCell.Directions,
                    _flagParser.OppositeDirection(d));
            }
        }

        public override void RemoveVertex(MazePoint p, Direction d)
        {
            if (Maze.TryGetValue(p, out MazeCell? startCell) && _movementHelper.CanMove(p, d, Size, out MazePoint final) && Maze.TryGetValue(final, out MazeCell? finalCell))
            {
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
            if (Maze.TryGetValue(p, out MazeCell? startCell))
            {
                return startCell.Directions;
            }
            throw new CellNotFoundException();
        }
    }
}

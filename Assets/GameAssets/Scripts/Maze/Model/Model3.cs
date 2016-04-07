using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class Model3 : ModelBase
    {
        private readonly IDirectionsFlagParser _flagParser;
        private readonly IMazePointFactory _pointFactory;
        private readonly IMovementHelper _movementHelper;

        private Dictionary<MazePoint, MazeCell> Maze { get; set; }

        public Model3(IDirectionsFlagParser flagParser, IMazePointFactory pointFactory, IMovementHelper movementHelper)
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
            foreach (var point in xValues.SelectMany(x =>  yValues.SelectMany(y => zValues.Select(z => _pointFactory.MakePoint(x, y, z)))))
            {
                Maze.Add(point, new MazeCell
                {
                    Directions = Direction.None
                });
            }
        }

        public override void PlaceVertex(MazePoint p, Direction d)
        {
            MazeCell startCell;
            MazePoint final;
            MazeCell finalCell;
            if (Maze.TryGetValue(p, out startCell) && _movementHelper.CanMove(p, d, Size, out final) && Maze.TryGetValue(final, out finalCell))
            {
                startCell.Directions = _flagParser.AddDirectionsToFlag(startCell.Directions, d);
                finalCell.Directions = _flagParser.AddDirectionsToFlag(finalCell.Directions,
                    _flagParser.OppositeDirection(d));
            }
        }

        public override void RemoveVertex(MazePoint p, Direction d)
        {
            MazeCell startCell;
            MazePoint final;
            MazeCell finalCell;
            if (Maze.TryGetValue(p, out startCell) && _movementHelper.CanMove(p, d, Size, out final) && Maze.TryGetValue(final, out finalCell))
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
            MazeCell startCell;
            if (Maze.TryGetValue(p, out startCell))
            {
                return startCell.Directions;
            }
            throw new CellNotFoundException();
        }
    }
}

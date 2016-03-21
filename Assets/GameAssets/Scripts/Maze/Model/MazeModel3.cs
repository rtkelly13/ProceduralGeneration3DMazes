using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class MazeModel3 : MazeBase, IBuilder
    {
        private readonly IMazePointFactory _pointFactory;
        private readonly IMovementHelper _movementHelper;
        private Dictionary<MazePoint, MazeCell> Maze { get; set; }

        public MazeModel3(IDirectionsFlagParser flagParser, IMazePointFactory pointFactory, IMovementHelper movementHelper) : base(flagParser)
        {
            _pointFactory = pointFactory;
            _movementHelper = movementHelper;
        }

        protected override void Initialise(MazeSize size)
        {
            Size = size;
            Maze = new Dictionary<MazePoint, MazeCell>();
            var xValues = Enumerable.Range(0, size.X).ToList();
            var yValues = Enumerable.Range(0, size.Z).ToList();
            var zValues = Enumerable.Range(0, size.Y).ToList();
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
                startCell.Directions = FlagParser.AddDirectionsToFlag(startCell.Directions, d);
                finalCell.Directions = FlagParser.AddDirectionsToFlag(finalCell.Directions,
                    FlagParser.OppositeDirection(d));
            }
        }

        public override void RemoveVertex(MazePoint p, Direction d)
        {
            MazeCell startCell;
            MazePoint final;
            MazeCell finalCell;
            if (Maze.TryGetValue(p, out startCell) && _movementHelper.CanMove(p, d, Size, out final) && Maze.TryGetValue(final, out finalCell))
            {
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
            MazeCell startCell;
            if (Maze.TryGetValue(p, out startCell))
            {
                return startCell.Directions;
            }
            throw new CellNotFoundException();
        }
    }
}

using System;
using ProceduralMaze.Maze.Helper;

namespace ProceduralMaze.Maze.Model
{
    public class DeadEndModelWrapper : IDeadEndModelWrapper
    {
        private readonly IMovementHelper _movementHelper;
        private readonly IDirectionsFlagParser _flagParser;

        private readonly IModelBuilder _model;
        private MazeCell[,,] Maze { get; set; }

        public DeadEndModelWrapper(IMovementHelper movementHelper, IDirectionsFlagParser flagParser, 
            IMazeArrayBuilder mazeArrayBuilder, IModelBuilder model)
        {
            _movementHelper = movementHelper;
            _flagParser = flagParser;
            _model = model;
            Maze = mazeArrayBuilder.Build(Size);
        }

        public bool HasDirections(MazePoint p, Direction d)
        {
            var baseResult = _model.HasDirections(p, d);
            if (baseResult)
            {
                return !_flagParser.FlagHasDirections(GetPoint(p).Directions, d);
            }
            return false;
        }

        public Direction GetFlagFromPoint(MazePoint p)
        {
            return _flagParser.RemoveDirectionsFromFlag(_model.GetFlagFromPoint(p), GetPoint(p).Directions);
        }

        public MazeSize Size => _model.Size;

        public MazePoint StartPoint => _model.StartPoint;

        public MazePoint EndPoint => _model.EndPoint;

        public void PlaceVertex(MazePoint p, Direction d)
        {
            throw new ArgumentException("You cannot add vertexes to a DeadEndModel");
        }

        public void RemoveVertex(MazePoint p, Direction d)
        {
            var startCell = GetPoint(p);
            if (_movementHelper.CanMove(p, d, Size, out var finalPoint))
            {
                var finalCell = Maze[finalPoint.X, finalPoint.Y, finalPoint.Z];
                startCell.Directions = _flagParser.AddDirectionsToFlag(startCell.Directions, d);
                finalCell.Directions = _flagParser.AddDirectionsToFlag(finalCell.Directions,
                    _flagParser.OppositeDirection(d));
            }
        }

        private MazeCell GetPoint(MazePoint p)
        {
            return Maze[p.X, p.Y, p.Z];
        }
    }
}

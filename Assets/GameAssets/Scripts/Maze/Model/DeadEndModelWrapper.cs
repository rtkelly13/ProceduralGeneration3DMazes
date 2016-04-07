using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class DeadEndModelWrapper: IDeadEndModelWrapper
    {
        private readonly IMovementHelper _movementHelper;
        private readonly IDirectionsFlagParser _flagParser;

        private readonly IModelBuilder _model;
        private MazeCell[,,] Maze { get; set; }

        public DeadEndModelWrapper(IMovementHelper movementHelper, IDirectionsFlagParser flagParser, IMazeArrayBuilder mazeArrayBuilder, IModelBuilder model)
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

        public MazeSize Size
        {
            get { return _model.Size; }
        }

        public MazePoint StartPoint
        {
            get { return _model.StartPoint; }
        }

        public MazePoint EndPoint
        {
            get { return _model.EndPoint; }
        }

        public void PlaceVertex(MazePoint p, Direction d)
        {
            throw new ArgumentException("You cannot add vertexes to a DeadEndModel");
        }

        public void RemoveVertex(MazePoint p, Direction d)
        {
            var startCell = GetPoint(p);
            MazePoint finalPoint;
            if (_movementHelper.CanMove(p, d, Size, out finalPoint))
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

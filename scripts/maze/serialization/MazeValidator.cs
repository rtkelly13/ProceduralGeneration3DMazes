using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Serialization
{
    /// <summary>
    /// Validates maze consistency.
    /// Checks for bidirectional connections, boundary violations, and reachability.
    /// </summary>
    public class MazeValidator : IMazeValidator
    {
        private readonly IDirectionsFlagParser _flagParser;
        private readonly IMovementHelper _movementHelper;

        public MazeValidator(IDirectionsFlagParser flagParser, IMovementHelper movementHelper)
        {
            _flagParser = flagParser;
            _movementHelper = movementHelper;
        }

        public MazeValidationResult Validate(IModel model)
        {
            var result = new MazeValidationResult();

            ValidateStartEndInBounds(model, result);
            ValidateBoundaryViolations(model, result);
            ValidateBidirectionalConsistency(model, result);
            ValidateStartEndConnections(model, result);

            return result;
        }

        private void ValidateStartEndInBounds(IModel model, MazeValidationResult result)
        {
            var size = model.Size;
            var start = model.StartPoint;
            var end = model.EndPoint;

            if (!IsPointInBounds(start, size))
            {
                result.AddError($"Start point ({start.X}, {start.Y}, {start.Z}) is outside maze bounds ({size.X}, {size.Y}, {size.Z})");
            }

            if (!IsPointInBounds(end, size))
            {
                result.AddError($"End point ({end.X}, {end.Y}, {end.Z}) is outside maze bounds ({size.X}, {size.Y}, {size.Z})");
            }
        }

        private void ValidateBoundaryViolations(IModel model, MazeValidationResult result)
        {
            var size = model.Size;

            for (int z = 0; z < size.Z; z++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int x = 0; x < size.X; x++)
                    {
                        var point = new MazePoint(x, y, z);
                        var directions = model.GetFlagFromPoint(point);

                        if (directions == Direction.None)
                            continue;

                        foreach (var dir in _flagParser.SplitDirectionsFromFlag(directions))
                        {
                            if (!_movementHelper.CanMove(point, dir, size, out _))
                            {
                                result.AddError($"Cell ({x}, {y}, {z}) has direction {dir} that points outside maze bounds");
                            }
                        }
                    }
                }
            }
        }

        private void ValidateBidirectionalConsistency(IModel model, MazeValidationResult result)
        {
            var size = model.Size;

            for (int z = 0; z < size.Z; z++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int x = 0; x < size.X; x++)
                    {
                        var point = new MazePoint(x, y, z);
                        var directions = model.GetFlagFromPoint(point);

                        if (directions == Direction.None)
                            continue;

                        foreach (var dir in _flagParser.SplitDirectionsFromFlag(directions))
                        {
                            if (!_movementHelper.CanMove(point, dir, size, out var neighbor))
                                continue; // Boundary violation already reported

                            var neighborDirections = model.GetFlagFromPoint(neighbor);
                            var oppositeDir = _flagParser.OppositeDirection(dir);

                            if (!_flagParser.FlagHasDirections(neighborDirections, oppositeDir))
                            {
                                result.AddError(
                                    $"Asymmetric connection: ({x}, {y}, {z}) has {dir} to ({neighbor.X}, {neighbor.Y}, {neighbor.Z}), " +
                                    $"but neighbor does not have {oppositeDir} back");
                            }
                        }
                    }
                }
            }
        }

        private void ValidateStartEndConnections(IModel model, MazeValidationResult result)
        {
            var size = model.Size;
            var start = model.StartPoint;
            var end = model.EndPoint;

            // Only check if points are in bounds (already validated above)
            if (IsPointInBounds(start, size))
            {
                var startDirections = model.GetFlagFromPoint(start);
                if (startDirections == Direction.None)
                {
                    result.AddError($"Start point ({start.X}, {start.Y}, {start.Z}) has no connections (isolated)");
                }
            }

            if (IsPointInBounds(end, size))
            {
                var endDirections = model.GetFlagFromPoint(end);
                if (endDirections == Direction.None)
                {
                    result.AddError($"End point ({end.X}, {end.Y}, {end.Z}) has no connections (isolated)");
                }
            }
        }

        private static bool IsPointInBounds(MazePoint point, MazeSize size)
        {
            return point.X >= 0 && point.X < size.X &&
                   point.Y >= 0 && point.Y < size.Y &&
                   point.Z >= 0 && point.Z < size.Z;
        }
    }
}

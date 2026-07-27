using System;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Factory
{
    public class MazeModelFactory : IMazeModelFactory
    {
        private readonly IMovementHelper _movementHelper;
        private readonly IDirectionsFlagParser _parser;
        private readonly IMazePointFactory _pointFactory;
        private readonly IMazeArrayBuilder _mazeArrayBuilder;
        private readonly IRandomPointGenerator _randomPointGenerator;

        public MazeModelFactory(IMovementHelper movementHelper, IDirectionsFlagParser parser, IMazePointFactory pointFactory, IMazeArrayBuilder mazeArrayBuilder, IRandomPointGenerator randomPointGenerator)
        {
            _movementHelper = movementHelper;
            _parser = parser;
            _pointFactory = pointFactory;
            _mazeArrayBuilder = mazeArrayBuilder;
            _randomPointGenerator = randomPointGenerator;
        }

        /// <summary>
        /// Maximum random draws before falling back to a deterministic choice. Generous enough
        /// that normal mazes always succeed on the first or second try.
        /// </summary>
        private const int MaxEndPointAttempts = 100;

        /// <summary>
        /// Picks an end point different from <paramref name="startPoint"/> where one exists.
        /// </summary>
        /// <remarks>
        /// Replaces an unbounded `while (start.Equals(end)) end = RandomPoint(...)` retry loop
        /// that **hung forever on a 1x1x1 maze**: a single-cell maze has exactly one point, so
        /// no distinct end point can ever be drawn. Reachable by importing a `.maze` file with
        /// `SIZE 1 1 1`, which the format permits.
        ///
        /// Two guards, because "retry until different" is unsafe in general:
        ///  * A single-cell maze returns the start point — degenerate but finite, and callers
        ///    already tolerate start == end (agents check for it before moving).
        ///  * Retries are bounded, then fall back to a deterministic neighbour. Random draws can
        ///    be arbitrarily unlucky, and with PickType.RandomEdge the candidate set is a small
        ///    subset of cells, so an unbounded loop is a latent stall even on larger mazes.
        /// </remarks>
        private MazePoint PickDistinctEndPoint(MazeSize size, PickType pickType, MazePoint startPoint)
        {
            if (size.X * size.Y * size.Z <= 1)
            {
                return startPoint;
            }

            for (var attempt = 0; attempt < MaxEndPointAttempts; attempt++)
            {
                var candidate = _randomPointGenerator.RandomPoint(size, pickType);
                if (!candidate.Equals(startPoint))
                {
                    return candidate;
                }
            }

            // Deterministic fallback: step one cell along whichever axis has room. The maze has
            // at least two cells, so exactly one of these must differ from the start.
            if (size.X > 1)
            {
                return _pointFactory.MakePoint(startPoint.X == 0 ? 1 : startPoint.X - 1, startPoint.Y, startPoint.Z);
            }

            if (size.Y > 1)
            {
                return _pointFactory.MakePoint(startPoint.X, startPoint.Y == 0 ? 1 : startPoint.Y - 1, startPoint.Z);
            }

            return _pointFactory.MakePoint(startPoint.X, startPoint.Y, startPoint.Z == 0 ? 1 : startPoint.Z - 1);
        }

        public IModelBuilder BuildMaze(MazeGenerationSettings settings)
        {
            var pickType = PickType.Random;
            if (settings.DoorsAtEdge)
            {
                pickType = PickType.RandomEdge;
            }
            var startPoint = _randomPointGenerator.RandomPoint(settings.Size, pickType);
            var endPoint = PickDistinctEndPoint(settings.Size, pickType, startPoint);
            var options = new ModelInitialisationOptions
            {
                Size = settings.Size,
                StartPoint = startPoint,
                EndPoint = endPoint
            };
            switch (settings.Option)
            {
                case MazeType.None:
                    throw new ArgumentException("Maze Type None is not supported");
                case MazeType.ArrayUnidirectional:
                    return new ArrayUnidirectionalModel(_parser, _movementHelper, _mazeArrayBuilder).BaseInitialise(options);
                case MazeType.ArrayBidirectional:
                    return new ArrayBidirectionalModel(_parser, _movementHelper, _mazeArrayBuilder).BaseInitialise(options);
                case MazeType.Dictionary:
                    return new DictionaryModel(_parser, _pointFactory, _movementHelper).BaseInitialise(options);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

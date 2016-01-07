using System.Collections.Generic;
using MazeGeneration.Factory;
using MazeGeneration.Helper;

namespace MazeGeneration.Model
{
    public abstract class MazeBase : IMaze
    {
        protected readonly IDirectionsFlagParser FlagParser;

        protected MazeBase(IDirectionsFlagParser flagParser)
        {
            FlagParser = flagParser;
        }

        public bool HasVertexes(MazePoint p, Direction flag)
        {
            return FlagParser.FlagHasDirections(GetFlagFromPoint(p), flag);
        }

        public abstract Direction GetFlagFromPoint(MazePoint p);

        public IEnumerable<Direction> GetsDirectionsFromPoint(MazePoint p)
        {
            return FlagParser.SplitDirectionsFromFlag(GetFlagFromPoint(p));
        }

        public void BaseInitialise(MazeSize size, bool allVertexes)
        {
            Size = size;
            Initialise(size, allVertexes);
        }

        protected abstract void Initialise(MazeSize size, bool allVertexes);

        public MazeSize Size { get; set; }
    }
}

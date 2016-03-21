using System.Collections;
using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Helper;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public abstract class MazeBase: IBuilder
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

        public abstract bool HasDirections(MazePoint p, Direction d);

        public abstract Direction GetFlagFromPoint(MazePoint p);

        public IEnumerable<Direction> GetsDirectionsFromPoint(MazePoint p)
        {
            return FlagParser.SplitDirectionsFromFlag(GetFlagFromPoint(p));
        }

        public abstract void PlaceVertex(MazePoint p, Direction d);
        public abstract void RemoveVertex(MazePoint p, Direction d);

        public MazeBase BaseInitialise(MazeSize size)
        {
            Size = size;
            Initialise(size);
            return this;
        }

        protected abstract void Initialise(MazeSize size);

        public MazeSize Size { get; set; }
    }
}

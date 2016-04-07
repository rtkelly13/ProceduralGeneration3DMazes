using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public interface IDirectionsFlagParser
    {
        IEnumerable<Direction> SplitDirectionsFromFlag(Direction d);
        bool FlagHasDirections(Direction flag, Direction d);

        Direction FlagUnion(Direction flag1, Direction flag2);
        Direction AddDirectionsToFlag(Direction flag, Direction d);

        Direction RemoveDirectionsFromFlag(Direction flag, Direction d);
        Direction OppositeDirection(Direction d);

        Direction OppositeFlag(Direction flag);

        bool IsDirection(Direction flag);

        List<Direction> Directions { get; }
    }
}
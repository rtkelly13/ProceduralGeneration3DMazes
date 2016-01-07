using System.Collections.Generic;

namespace MazeGeneration.Helper
{
    public interface IDirectionsFlagParser
    {
        IEnumerable<Direction> SplitDirectionsFromFlag(Direction d);
        bool FlagHasDirections(Direction flag, Direction d);
        Direction AddDirectionsToFlag(Direction flag, Direction d);
        Direction RemoveDirectionsFromFlag(Direction flag, Direction d);
        Direction OppositeDirection(Direction d);

        bool IsDirection(Direction flag);
    }
}
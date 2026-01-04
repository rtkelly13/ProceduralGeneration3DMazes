using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Helper
{
    public interface IDirectionsFlagParser
    {
        Direction[] SplitDirectionsFromFlag(Direction d);
        bool FlagHasDirections(Direction flag, Direction d);

        Direction FlagUnion(Direction flag1, Direction flag2);
        Direction AddDirectionsToFlag(Direction flag, Direction d);

        Direction RemoveDirectionsFromFlag(Direction flag, Direction d);
        Direction OppositeDirection(Direction d);

        Direction OppositeFlag(Direction flag);

        bool IsDirection(Direction flag);

        IReadOnlyList<Direction> Directions { get; }
    }
}

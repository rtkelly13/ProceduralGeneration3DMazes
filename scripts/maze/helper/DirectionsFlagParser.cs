using System;
using System.Collections.Generic;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Helper
{
    public class DirectionsFlagParser : IDirectionsFlagParser
    {
        private static readonly Direction[] _allDirections =
        [
            Direction.Left,
            Direction.Right,
            Direction.Forward,
            Direction.Back,
            Direction.Up,
            Direction.Down
        ];

        public IReadOnlyList<Direction> Directions => _allDirections;

        public Direction[] SplitDirectionsFromFlag(Direction d)
        {
            Span<Direction> buffer = stackalloc Direction[6];
            int count = 0;
            foreach (var dir in _allDirections)
            {
                if ((d & dir) != 0)
                    buffer[count++] = dir;
            }
            return buffer[..count].ToArray();
        }

        public bool IsDirection(Direction flag)
        {
            foreach (var dir in _allDirections)
            {
                if (dir == flag) return true;
            }
            return false;
        }

        public bool FlagHasDirections(Direction flag, Direction d)
        {
            return (flag & d) == d;
        }

        public Direction AddDirectionsToFlag(Direction flag, Direction d)
        {
            return flag | d;
        }

        public Direction FlagUnion(Direction flag1, Direction flag2)
        {
            return flag1 & flag2;
        }

        public Direction FlagUnions(params Direction[] directions)
        {
            Direction result = Direction.None;
            foreach (var dir in directions)
                result = FlagUnion(result, dir);
            return result;
        }

        public Direction AddDirectionsTogether(params Direction[] directions)
        {
            Direction result = Direction.None;
            foreach (var dir in directions)
                result = AddDirectionsToFlag(result, dir);
            return result;
        }

        public Direction RemoveDirectionsFromFlag(Direction flag, Direction d)
        {
            return flag & ~d;
        }

        public Direction OppositeFlag(Direction flag)
        {
            return RemoveDirectionsFromFlag(Direction.All, flag);
        }

        public Direction OppositeDirection(Direction d)
        {
            return d switch
            {
                Direction.Left or Direction.Right => Direction.XAxis & ~d,
                Direction.Back or Direction.Forward => Direction.YAxis & ~d,
                Direction.Up or Direction.Down => Direction.ZAxis & ~d,
                _ => throw new ArgumentException("Not a valid direction")
            };
        }
    }
}

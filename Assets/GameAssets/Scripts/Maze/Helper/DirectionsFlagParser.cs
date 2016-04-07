using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public class DirectionsFlagParser : IDirectionsFlagParser
    {
        public List<Direction> Directions { get; private set; }

        public DirectionsFlagParser()
        {
            Directions = new List<Direction>
            {
                Direction.Left,
                Direction.Right,
                Direction.Forward,
                Direction.Back,
                Direction.Up,
                Direction.Down
            };
        }

        public IEnumerable<Direction> SplitDirectionsFromFlag(Direction d)
        {
            return Directions.Where(possibleDirection => (d & possibleDirection) != 0); 
        }

        public bool IsDirection(Direction flag)
        {
            return Directions.Any(x => x == flag);
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
            return directions.Aggregate(Direction.None, FlagUnion);
        }

        public Direction AddDirectionsTogether(params Direction[] directions)
        {
            return directions.Aggregate(Direction.None, AddDirectionsToFlag);
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
            switch (d)
            {
                case Direction.Left:
                case Direction.Right:
                    return Direction.XAxis & ~d;
                case Direction.Back:
                case Direction.Forward:
                    return Direction.YAxis & ~d;
                case Direction.Up:
                case Direction.Down:
                    return Direction.ZAxis & ~d;
            }
            throw new ArgumentException("Not a valid direction");
        }
    }
}

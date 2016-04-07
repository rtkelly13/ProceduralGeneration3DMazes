using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    /*
        Using Relative Directions https://en.wikipedia.org/wiki/Relative_direction
        Increasing directions are Right, Forward and Up
    */
    public class MovementHelper : IMovementHelper
    {
        private readonly IDirectionsFlagParser _flagParser;
        private readonly IMazePointFactory _pointFactory;
        private readonly IPointValidity _pointValidity;

        public MovementHelper(IDirectionsFlagParser flagParser, IMazePointFactory pointFactory, IPointValidity pointValidity)
        {
            _flagParser = flagParser;
            _pointFactory = pointFactory;
            _pointValidity = pointValidity;
        }

        public IEnumerable<Direction> AdjacentPoints(MazePoint p, MazeSize size)
        {
            if (p.X < size.X - 1)
            {
                yield return Direction.Right;
            }
            if (p.X > 0)
            {
                yield return Direction.Left;
            }
            
            if (p.Y < size.Y - 1)
            {
                yield return Direction.Forward;
            }
            if (p.Y > 0)
            {
                yield return Direction.Back;
            }

            if (p.Z < size.Z - 1)
            {
                yield return Direction.Up;
            }
            if (p.Z > 0)
            {
                yield return Direction.Down;
            }

        }

        public Direction AdjacentPointsFlag(MazePoint p, MazeSize size)
        {
            //return AdjacentPoints(p, size).Aggregate(Direction.None, (seed, item) => _flagParser.AddDirectionsToFlag(seed, item));
            var flag = Direction.None;
            if (p.X < size.X - 1)
            {
                flag = _flagParser.AddDirectionsToFlag(flag, Direction.Right);
            }
            if (p.X > 0)
            {
                flag = _flagParser.AddDirectionsToFlag(flag, Direction.Left);
            }

            if (p.Y < size.Y - 1)
            {
                flag =_flagParser.AddDirectionsToFlag(flag, Direction.Forward);
            }
            if (p.Y > 0)
            {
                flag = _flagParser.AddDirectionsToFlag(flag, Direction.Back);
            }

            if (p.Z < size.Z - 1)
            {
                flag = _flagParser.AddDirectionsToFlag(flag, Direction.Up);
            }
            if (p.Z > 0)
            {
                flag = _flagParser.AddDirectionsToFlag(flag, Direction.Down);
            }
            return flag;
            
        }

        public MazePoint Move(MazePoint start, Direction d, MazeSize size)
        {
            MazePoint final;
            if (CanMove(start, d, size, out final))
            {
                return final;
            }
            throw new ArgumentException("Cannot move in direction");
        }

        public bool CanMove(MazePoint start, Direction d, MazeSize size, out MazePoint final)
        {
            switch (d)
            {
                case Direction.None:
                    final = start;
                    break;
                case Direction.Right:
                    final = _pointFactory.MakePoint(start.X + 1, start.Y, start.Z);
                    break;
                case Direction.Left:
                    final = _pointFactory.MakePoint(start.X - 1, start.Y, start.Z);
                    break;
                case Direction.Forward:
                    final = _pointFactory.MakePoint(start.X, start.Y + 1, start.Z);
                    break;
                case Direction.Back:
                    final = _pointFactory.MakePoint(start.X, start.Y - 1, start.Z);
                    break;
                case Direction.Up:
                    final = _pointFactory.MakePoint(start.X, start.Y, start.Z + 1);
                    break;
                case Direction.Down:
                    final = _pointFactory.MakePoint(start.X, start.Y, start.Z - 1);
                    break;
                default:
                    throw new ArgumentException("Unsupported movement direction");
            }
            return _pointValidity.ValidPoint(final, size);
        }

        public MazePoint Move(MazePoint start, MazePoint final, MazeSize size)
        {
            if (CanMove(start, final, size))
            {
                return final;
            }
            throw new ArgumentException("Invalid Point to move to");
        }

        public bool CanMove(MazePoint start, MazePoint final, MazeSize size)
        {
            return _pointValidity.ValidPoint(final, size);
        }
    }
}

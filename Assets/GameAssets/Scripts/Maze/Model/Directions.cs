using System;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    /// <summary>
    /// Specifies directions along thee axes
    /// </summary>
    [Flags]
    public enum Direction
    {
        None = 0,

        Left = 1,
        Right = 2,
        Down = 4,
        Up = 8,
        Back = 16,
        Forward = 32,

        XAxis = Left | Right,
        YAxis = Back | Forward,
        ZAxis = Down | Up,

        All = Left | Right | Down | Up | Back | Forward
    }

}

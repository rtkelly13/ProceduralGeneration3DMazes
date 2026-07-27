using System;

namespace ProceduralMaze.Maze.Helper
{
    /// <summary>Real wall-clock time. The only place in maze logic that reads the clock.</summary>
    public class SystemClock : ISystemClock
    {
        public DateTime Now => DateTime.Now;
    }
}

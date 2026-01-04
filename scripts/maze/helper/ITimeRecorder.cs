using System;

namespace ProceduralMaze.Maze.Helper
{
    public interface ITimeRecorder
    {
        TimeSpan GetRunningTime(Action time);
        string GetStringFromTime(TimeSpan span);
    }
}

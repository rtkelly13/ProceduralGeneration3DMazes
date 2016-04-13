using System;

namespace Assets.GameAssets.Scripts.Maze.Helper
{
    public interface ITimeRecorder
    {
        TimeSpan GetRunningTime(Action time);
        string GetStringFromTime(TimeSpan span);
    }
}
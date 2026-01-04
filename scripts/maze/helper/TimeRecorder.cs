using System;
using System.Diagnostics;

namespace ProceduralMaze.Maze.Helper
{
    public class TimeRecorder : ITimeRecorder
    {
        public TimeSpan GetRunningTime(Action time)
        {
            var stopWatch = Stopwatch.StartNew();
            time();
            stopWatch.Stop();
            return stopWatch.Elapsed;
        }

        public string GetStringFromTime(TimeSpan span)
        {
            return $"{span.Hours}:{span.Minutes}:{span.Seconds}:{span.Milliseconds}";
        }
    }
}

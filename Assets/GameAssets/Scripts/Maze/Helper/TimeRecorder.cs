using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.Maze.Helper
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
            return String.Format("{0}:{1}:{2}:{3}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
        }
    }
}

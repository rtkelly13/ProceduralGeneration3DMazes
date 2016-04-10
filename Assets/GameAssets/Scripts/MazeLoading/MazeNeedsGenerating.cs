using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.MazeLoading
{
    public class MazeNeedsGenerating : IMazeNeedsGenerating
    {
        private static bool _generate;

        public bool Generate
        {
            get { return _generate; }
            set { _generate = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class AlgorithmSettings
    {
        public Algorithm Algorithm { get; set; }
        public MazeSize Size { get; set; }
        public ModelOption Option { get; set; }
    }
}

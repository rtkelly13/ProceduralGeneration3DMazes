using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class CurrentSettingsHolder : ICurrentSettingsHolder
    {
        private static MazeGenerationSettings _settings;

        public MazeGenerationSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }
    }
}

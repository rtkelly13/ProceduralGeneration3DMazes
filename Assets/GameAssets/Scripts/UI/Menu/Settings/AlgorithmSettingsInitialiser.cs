using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class AlgorithmSettingsInitialiser : IAlgorithmSettingsInitialiser
    {
        public void InitialiseOver(AlgorithmSettings specificSettings, AlgorithmSettings baseSettings)
        {
            specificSettings.Size = baseSettings.Size ?? new MazeSize();
            specificSettings.Option = baseSettings.Option;
            specificSettings.Algorithm = baseSettings.Algorithm;
        }
    }
}

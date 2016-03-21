using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class AlgorithmSettingsInitialiser : IAlgorithmSettingsInitialiser
    {
        public void InitialiseOver(MazeGenerationSettings specificSettings, MazeGenerationSettings baseSettings)
        {
            specificSettings.Size = baseSettings.Size ?? new MazeSize();
            specificSettings.Option = baseSettings.Option;
            specificSettings.Algorithm = baseSettings.Algorithm;
        }
    }
}

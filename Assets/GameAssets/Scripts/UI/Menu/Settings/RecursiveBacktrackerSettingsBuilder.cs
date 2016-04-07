using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class RecursiveBacktrackerSettingsBuilder: ISettingBuilder
    {
        public Algorithm AlgorithmType { get; private set; }
        private MazeGenerationSettings _settings;

        public RecursiveBacktrackerSettingsBuilder()
        {
            AlgorithmType = Algorithm.RecursiveBacktrackerAlgorithm;
        }
        
        

        public void BuildMenu(Transform transform, MazeGenerationSettings existingSettings, Action<MazeGenerationSettings> settingsChanged)
        {
            _settings = existingSettings;
        }

        public MazeGenerationSettings GetSettings()
        {
            return _settings;
        }

        public void Dispose()
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class SettingsBuilder : ISettingsBuilder
    {
        private readonly List<ISettingBuilder> _settingsBuilders;

        public SettingsBuilder(List<ISettingBuilder> settingsBuilders)
        {
            _settingsBuilders = settingsBuilders;
        }

        public void BuildMenu(Transform transform, Algorithm algorithm, MazeGenerationSettings existingSettings, Action<MazeGenerationSettings> settingsChanged)
        {
            transform.Clear();
            GetStrategy(algorithm).BuildMenu(transform, existingSettings, settingsChanged);
        }

        public MazeGenerationSettings GetSettings(Algorithm algorithm)
        {
            return GetStrategy(algorithm).GetSettings();
        }

        private ISettingBuilder GetStrategy(Algorithm algorithm)
        {
            var settingsStrategy = _settingsBuilders.FirstOrDefault(x => x.AlgorithmType == algorithm);
            if (settingsStrategy != null)
            {
                return settingsStrategy;
            }
            else
            {
                throw new ArgumentException("Unsupported Algorithm Type");
            }
        }
    }
}
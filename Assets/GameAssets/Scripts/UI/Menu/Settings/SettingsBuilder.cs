using System;
using System.Collections.Generic;
using System.Linq;
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

        public void BuildMenu(Transform transform, Algorithm algorithm, AlgorithmSettings existingSettings, Action<AlgorithmSettings> settingsChanged)
        {
            transform.Clear();
            GetStrategy(algorithm).BuildMenu(transform, existingSettings, settingsChanged);
        }

        public AlgorithmSettings GetSettings(Algorithm algorithm)
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
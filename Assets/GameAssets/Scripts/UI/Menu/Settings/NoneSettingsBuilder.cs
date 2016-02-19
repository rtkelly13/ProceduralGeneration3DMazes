using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Examples.UI;
using Assets.GameAssets.Scripts.UI.Helper;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class NoneSettingsBuilder: ISettingBuilder
    {
        public Algorithm AlgorithmType { get; private set; }

        private readonly IResourceLoader _resourceLoader;
        private AlgorithmSettings settings;

        public NoneSettingsBuilder(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
            AlgorithmType = Algorithm.None;
        }

        public void BuildMenu(Transform transform, AlgorithmSettings existingSettings, Action<AlgorithmSettings> settingsChanged)
        {
            settings = existingSettings;
        }

        public AlgorithmSettings GetSettings()
        {
            return settings;
        }

        public void Dispose()
        {
            
        }
    }
}

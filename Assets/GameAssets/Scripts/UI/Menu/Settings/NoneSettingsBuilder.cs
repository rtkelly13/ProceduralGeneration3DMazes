using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Examples.UI;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Helper;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class NoneSettingsBuilder: ISettingBuilder
    {
        public Algorithm AlgorithmType { get; private set; }

        private readonly IResourceLoader _resourceLoader;
        private MazeGenerationSettings settings;

        public NoneSettingsBuilder(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
            AlgorithmType = Algorithm.None;
        }

        public void BuildMenu(Transform transform, MazeGenerationSettings existingSettings, Action<MazeGenerationSettings> settingsChanged)
        {
            settings = existingSettings;
        }

        public MazeGenerationSettings GetSettings()
        {
            return settings;
        }

        public void Dispose()
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public interface ISettingBuilder: IDisposable
    {
        Algorithm AlgorithmType { get; }
        void BuildMenu(Transform transform, MazeGenerationSettings existingSettings, Action<MazeGenerationSettings> settingsChanged);
        MazeGenerationSettings GetSettings();
    }
}

using System;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public interface ISettingsBuilder
    {
        void BuildMenu(Transform transform, Algorithm algorithm, MazeGenerationSettings existingSettings, Action<MazeGenerationSettings> settingsChanged);
    }
}
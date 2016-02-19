using System;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public interface ISettingsBuilder
    {
        void BuildMenu(Transform transform, Algorithm algorithm, AlgorithmSettings existingSettings, Action<AlgorithmSettings> settingsChanged);
    }
}
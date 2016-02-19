using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public interface ISettingBuilder: IDisposable
    {
        Algorithm AlgorithmType { get; }
        void BuildMenu(Transform transform, AlgorithmSettings existingSettings, Action<AlgorithmSettings> settingsChanged);
        AlgorithmSettings GetSettings();
    }
}

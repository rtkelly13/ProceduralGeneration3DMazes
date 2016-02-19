using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.UI.Menu.Validation
{
    public interface IValidateSetting : IBaseValidateSettings
    {
        Algorithm AlgorithmType { get; }
    }
}

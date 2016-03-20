using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public class CurrentSettingsHolder : ICurrentSettingsHolder
    {
        public AlgorithmSettings Settings { get; set; }
    }
}

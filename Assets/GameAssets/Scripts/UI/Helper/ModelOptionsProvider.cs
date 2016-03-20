using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.UI.Controls;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public class ModelOptionsProvider : IModelOptionsProvider
    { 
        public List<DropdownOption<string, ModelOption>> DropdownOptions { get; private set; }

        public ModelOptionsProvider()
        {
            DropdownOptions = new List<DropdownOption<string, ModelOption>>()
            {
                new DropdownOption<string, ModelOption>("None", ModelOption.None),
                new DropdownOption<string, ModelOption>("Option 1", ModelOption.Option1),
                new DropdownOption<string, ModelOption>("Option2", ModelOption.Option3),
                new DropdownOption<string, ModelOption>("Option3", ModelOption.Option3),
            };
        }
    }
}

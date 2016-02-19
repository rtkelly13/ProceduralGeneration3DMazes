 using System.Collections.Generic;
using Assets.GameAssets.Scripts.UI.Controls;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public interface IModelOptionsProvider
    {
        List<DropdownOption<string, ModelOption>> DropdownOptions { get; }
    }
}
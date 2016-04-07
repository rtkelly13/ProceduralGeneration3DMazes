using System.Collections.Generic;
using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public interface IWallCarverOptionsProvider
    {
        List<DropdownOption<string, WallCarverOption>> DropdownOptions { get; }
    }
}
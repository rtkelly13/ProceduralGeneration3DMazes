using System.Collections.Generic;
using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public interface IAlgorithmsProvider
    {
        List<DropdownOption<string, Algorithm>> DropdownOptions { get; }
    }
}
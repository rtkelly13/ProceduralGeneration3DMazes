using System.Collections.Generic;
using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public interface IYesNoOptionsProvider
    {
        List<DropdownOption<string, bool>> DropdownOptions { get; }
    }
}
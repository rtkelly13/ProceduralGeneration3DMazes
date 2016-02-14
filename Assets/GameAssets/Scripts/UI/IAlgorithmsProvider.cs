using System.Collections.Generic;

namespace Assets.GameAssets.Scripts.UI
{
    public interface IAlgorithmsProvider
    {
        List<DropdownOption<string, Algorithm>> DropdownOptions { get; }
    }
}
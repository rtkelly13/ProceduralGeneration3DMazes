using System.Collections.Generic;
using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public class YesNoOptionsProvider : IYesNoOptionsProvider
    {
        public List<DropdownOption<string, bool>> DropdownOptions { get; private set; }

        public YesNoOptionsProvider()
        {
            DropdownOptions = new List<DropdownOption<string, bool>>()
            {
                new DropdownOption<string, bool>("No", false),
                new DropdownOption<string, bool>("Yes", true),
            };
        }
    }
}
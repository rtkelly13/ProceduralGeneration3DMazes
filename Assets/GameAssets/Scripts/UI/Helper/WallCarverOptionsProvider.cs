using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public class WallCarverOptionsProvider : IWallCarverOptionsProvider
    {
        public List<DropdownOption<string, WallCarverOption>> DropdownOptions { get; private set; }

        public WallCarverOptionsProvider()
        {
            DropdownOptions = new List<DropdownOption<string, WallCarverOption>>()
            {
                new DropdownOption<string, WallCarverOption>("None", WallCarverOption.None),
                new DropdownOption<string, WallCarverOption>("Random", WallCarverOption.Random),
                new DropdownOption<string,WallCarverOption>("Dead End", WallCarverOption.DeadEnd)
            };
        }
    }
}

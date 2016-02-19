using System.Collections.Generic;
using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public class AlgorithmsProvider : IAlgorithmsProvider
    {
        public List<DropdownOption<string, Algorithm>> DropdownOptions { get; private set; }

        public AlgorithmsProvider()
        {
            DropdownOptions = new List<DropdownOption <string, Algorithm>>()
            {
                new DropdownOption<string, Algorithm>("None", Algorithm.None),
                new DropdownOption<string, Algorithm>("Growing Tree Algorithm", Algorithm.GrowingTreeAlgorithm)
            };
        }
        
    }
}

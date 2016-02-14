using System.Collections.Generic;

namespace Assets.GameAssets.Scripts.UI
{
    public class AlgorithmsProvider : IAlgorithmsProvider
    {
        public List<DropdownOption<string, Algorithm>> DropdownOptions { get; private set; }

        public AlgorithmsProvider()
        {
            DropdownOptions = new List<DropdownOption <string, Algorithm>>()
            {
                new DropdownOption<string, Algorithm>("None", Algorithm.GrowingTreeAlgorithm),
                new DropdownOption<string, Algorithm>("Growing Tree Algorithm", Algorithm.GrowingTreeAlgorithm)
            };
        }
        
    }
}

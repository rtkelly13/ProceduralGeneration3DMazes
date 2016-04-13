using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public class AgentOptionsProvider : IAgentOptionsProvider
    {
        public List<DropdownOption<string, AgentType>> DropdownOptions { get; private set; }

        public AgentOptionsProvider()
        {
            DropdownOptions = new List<DropdownOption<string, AgentType>>()
            {
                new DropdownOption<string, AgentType>("None", AgentType.None),
                new DropdownOption<string, AgentType>("Random", AgentType.Random),
                new DropdownOption<string, AgentType>("Recall", AgentType.Perfect),
            };
        }
    }
}

using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Agents;
using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public interface IAgentOptionsProvider
    {
        List<DropdownOption<string, AgentType>> DropdownOptions { get; }
    }
}
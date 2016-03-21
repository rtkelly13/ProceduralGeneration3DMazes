 using System.Collections.Generic;
 using Assets.GameAssets.Scripts.Maze.Factory;
 using Assets.GameAssets.Scripts.UI.Controls;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public interface IModelOptionsProvider
    {
        List<DropdownOption<string, MazeType>> DropdownOptions { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.UI.Controls;
using Assets.GameAssets.Scripts.UI.Menu.Settings;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public class ModelOptionsProvider : IModelOptionsProvider
    {
        public List<DropdownOption<string, MazeType>> DropdownOptions { get; private set; }

        public ModelOptionsProvider()
        {
            DropdownOptions = new List<DropdownOption<string, MazeType>>()
            {
                new DropdownOption<string, MazeType>("None", MazeType.None),
                //new DropdownOption<string, MazeType>("Undirected Maze", MazeType.UndirectedMaze),
                new DropdownOption<string, MazeType>("Directed Maze", MazeType.DirectedMaze),
                new DropdownOption<string, MazeType>("Dictionary", MazeType.DictionaryMaze),
            };
        }
    }
}

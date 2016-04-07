using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeLoading
{
    public class CurrentMazeHolder : ICurrentMazeHolder
    {
        private static IMazeJumper _carver;

        public IMazeJumper MazeJumper
        {
            get { return _carver; }
            set { _carver = value; }
        } 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeLoading
{
    public class CurrentMazeHolder : ICurrentMazeHolder
    {
        private static MazeGenerationResults _results;

        public MazeGenerationResults Results
        {
            get { return _results; }
            set { _results = value; }
        } 
    }
}

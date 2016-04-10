﻿using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class AlgorithmRunResults
    {
        public IMazeCarver Carver { get; set; }
        public List<Direction> DirectionsCarvedIn { get; set; }  
    }
}
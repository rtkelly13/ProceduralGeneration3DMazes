using System;
using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace ProcGenMaze.Experiments
{
    public class ExperimentResult
    {
        public double AverageShortestPath { get; set; }
        public TimeSpan AverageGenerationTime { get; set; }
        public Dictionary<Direction, double> DirectionWeightings { get; set; }
        public double AverageCellsFilledIn { get; set; }
    }
}
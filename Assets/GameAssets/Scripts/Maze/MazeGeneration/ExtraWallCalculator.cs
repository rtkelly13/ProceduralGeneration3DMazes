using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Factory;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class ExtraWallCalculator : IExtraWallCalculator
    {
        public int Calulate(MazeSize size)
        {
            return (int)Math.Floor(Math.Sqrt(size.X*size.Y*size.Z) / 2);
        }
    }
}

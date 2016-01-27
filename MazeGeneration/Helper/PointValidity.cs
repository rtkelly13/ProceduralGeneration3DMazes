using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MazeGeneration.Factory;
using MazeGeneration.Model;

namespace MazeGeneration.Helper
{
    public class PointValidity : IPointValidity
    {
        public bool ValidPoint(MazePoint p, MazeSize size)
        {
            if (p.X < 0 || p.X > size.Width - 1)
            {
                return false;
            }
            if (p.Y < 0 || p.Y > size.Height - 1)
            {
                return false;
            }
            if (p.Z < 0 || p.Z > size.Depth - 1)
            {
                return false;
            }
            return true;
        }
    }
}

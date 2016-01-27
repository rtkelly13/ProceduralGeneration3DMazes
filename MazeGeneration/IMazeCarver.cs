using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeGeneration
{
    public interface IMazeCarver: IMazeJumper
    {
        IEnumerable<Direction> CarvableDirections();
        Direction CarvableFlag();

        IEnumerable<Direction> AlreadyCarvedDirections();
        Direction AlreadyCarvedFlag();

        bool CanCarveInDirection(Direction d);
        void CarveInDirection(Direction d);
        bool AlreadyCarvedDirection(Direction d);

        IMazeJumper CarvingFinished();
    }
}

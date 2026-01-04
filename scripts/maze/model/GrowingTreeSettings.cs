using System.Collections.Generic;
using ProceduralMaze.Maze.Generation;

namespace ProceduralMaze.Maze.Model
{
    public class GrowingTreeSettings
    {
        public List<GrowingTreeStrategy> Strategies { get; set; } = new List<GrowingTreeStrategy> { GrowingTreeStrategy.Newest };
        public int NewestWeight { get; set; } = 100;
        public int OldestWeight { get; set; } = 0;
        public int RandomWeight { get; set; } = 0;
    }
}

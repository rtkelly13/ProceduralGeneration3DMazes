using System.Collections.Generic;

namespace ProceduralMaze.Maze.Generation
{
    public class DeadEndFillerResult
    {
        public List<CarvedCellResult> CellsFilledIn { get; set; } = new();
        public int TotalCellsFilledIn { get; set; }
    }
}

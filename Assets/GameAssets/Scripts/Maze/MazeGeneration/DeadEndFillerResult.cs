using System.Collections.Generic;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public class DeadEndFillerResult
    {
        public List<CarvedCellResult> CellsFilledIn { get; set; }
        public int TotalCellsFilledIn { get; set; }
    }
}
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public class WallRemovalCalculator : IWallRemovalCalculator
    {
        /// <summary>
        /// Calculates the number of internal walls that can be removed from a perfect maze.
        /// 
        /// In a perfect maze (spanning tree), exactly (totalCells - 1) passages are carved.
        /// The remaining internal walls = totalInternalWalls - (totalCells - 1).
        /// </summary>
        public int Calculate(MazeSize size)
        {
            int x = size.X;
            int y = size.Y;
            int z = size.Z;

            // Total cells in the maze
            int totalCells = x * y * z;

            // Total internal walls in the grid (walls between adjacent cells)
            // Walls between X neighbors: (X-1) * Y * Z
            // Walls between Y neighbors: X * (Y-1) * Z
            // Walls between Z neighbors: X * Y * (Z-1)
            int totalInternalWalls = (x - 1) * y * z + x * (y - 1) * z + x * y * (z - 1);

            // In a perfect maze, exactly (totalCells - 1) walls are removed to create passages
            // So the remaining walls that could still be removed:
            int removableWalls = totalInternalWalls - (totalCells - 1);

            return removableWalls > 0 ? removableWalls : 0;
        }
    }
}

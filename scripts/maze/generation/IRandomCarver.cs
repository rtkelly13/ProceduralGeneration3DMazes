using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Generation
{
    public interface IRandomCarver
    {
        void CarveRandomWalls(IMazeCarver carver, WallCarverOption option, int numberOfWalls);
    }
}

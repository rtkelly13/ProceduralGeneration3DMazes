using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.UI.Helper;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public interface IRandomCarver
    {
        void CarveRandomWalls(IMazeCarver carver, WallCarverOption option, int numberOfWalls);
    }
}
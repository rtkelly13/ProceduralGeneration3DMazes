using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public interface ILoadCellPrefab
    {
        MazeCellPrefab GetPrefab(Direction flag);
    }
}
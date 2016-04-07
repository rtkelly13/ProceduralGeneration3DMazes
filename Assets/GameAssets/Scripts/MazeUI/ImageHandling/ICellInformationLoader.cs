using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public interface ICellInformationLoader
    {
        CellSpriteNames LoadCellSpriteNames(Direction flag);
    }
}
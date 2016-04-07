using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeLoading
{
    public interface ICurrentMazeHolder
    {
        IMazeJumper MazeJumper { get; set; }
    }
}
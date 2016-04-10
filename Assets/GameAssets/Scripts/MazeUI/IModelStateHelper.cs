using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public interface IModelStateHelper
    {
        void SetNextModelState(IModelState modelState);
    }
}
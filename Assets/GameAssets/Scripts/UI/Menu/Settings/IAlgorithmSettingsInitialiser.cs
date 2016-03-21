using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings
{
    public interface IAlgorithmSettingsInitialiser
    {
        void InitialiseOver(MazeGenerationSettings specificSettings, MazeGenerationSettings baseSettings);
    }
}
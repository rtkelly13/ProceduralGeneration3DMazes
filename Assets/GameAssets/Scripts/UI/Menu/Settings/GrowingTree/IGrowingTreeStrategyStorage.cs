using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings.GrowingTree
{
    public interface IGrowingTreeStrategyStorage
    {
        void AddOrUpdate(GrowingTreeStrategy strategy, int total);
        int Get(GrowingTreeStrategy strategy);
        IEnumerable<GrowingTreeStrategy> GetAllStrategies();
        void LoadStrategies(List<GrowingTreeStrategy> strategies);
    }
}
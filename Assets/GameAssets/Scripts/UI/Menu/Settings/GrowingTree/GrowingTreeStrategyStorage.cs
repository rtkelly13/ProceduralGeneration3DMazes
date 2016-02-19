using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;

namespace Assets.GameAssets.Scripts.UI.Menu.Settings.GrowingTree
{
    public class GrowingTreeStrategyStorage : IGrowingTreeStrategyStorage
    {
        private readonly Dictionary<GrowingTreeStrategy, int> _strategies = new Dictionary<GrowingTreeStrategy, int>();

        private readonly object _objLock = new object();

        public IEnumerable<GrowingTreeStrategy> GetAllStrategies()
        {
            lock (_objLock)
            {
                return _strategies.SelectMany(x => Enumerable.Repeat(x.Key, x.Value));
            }
        }

        public void AddOrUpdate(GrowingTreeStrategy strategy, int total)
        {
            lock (_objLock)
            {
                if (_strategies.ContainsKey(strategy))
                {
                    _strategies.Remove(strategy);
                }
                _strategies.Add(strategy, total);
            }
        }

        public int Get(GrowingTreeStrategy strategy)
        {
            lock (_objLock)
            {
                int result;
                return _strategies.TryGetValue(strategy, out result) ? result : 0;
            }
        }

        public void LoadStrategies(List<GrowingTreeStrategy> strategies)
        {
            strategies = strategies ?? new List<GrowingTreeStrategy>();
            foreach (var value in Enum.GetValues(typeof(GrowingTreeStrategy)))
            {
                var strategy = (GrowingTreeStrategy) value;
                AddOrUpdate(strategy, strategies.Count(x => x == strategy));
            }
        }
    }
}

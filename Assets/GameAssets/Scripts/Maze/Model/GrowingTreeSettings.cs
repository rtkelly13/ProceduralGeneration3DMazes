using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;

namespace Assets.GameAssets.Scripts.Maze.Model
{
    public class GrowingTreeSettings: MazeGenerationSettings
    {
        public List<GrowingTreeStrategy> Strategies;
    }
}

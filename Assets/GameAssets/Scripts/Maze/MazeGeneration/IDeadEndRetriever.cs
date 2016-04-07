using System.Collections.Generic;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.MazeGeneration
{
    public interface IDeadEndRetriever
    {
        IEnumerable<MazePoint> GetDeadEnds(IMazeCarver mazeCarver);
    }
}
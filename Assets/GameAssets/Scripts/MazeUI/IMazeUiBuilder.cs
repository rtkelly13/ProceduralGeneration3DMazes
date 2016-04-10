using Assets.GameAssets.Scripts.Maze.Factory;
using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.Maze.Solver;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public interface IMazeUiBuilder
    {
        void BuildMazeUI(Transform mazeField, MazeGenerationResults results, int z, UiMode mode);
    }
}
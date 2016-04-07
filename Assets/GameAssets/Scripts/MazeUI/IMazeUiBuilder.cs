using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public interface IMazeUiBuilder
    {
        void BuildMazeUI(Transform mazeField, IMazeJumper maze, int z);
    }
}
using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public interface ILineDrawer
    {
        void DrawLine(Transform mazeField, MazePoint p, Direction d, int z, LineColour colour);
    }
}
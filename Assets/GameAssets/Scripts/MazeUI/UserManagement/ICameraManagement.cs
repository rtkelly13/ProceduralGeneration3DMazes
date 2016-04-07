using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.UserManagement
{
    public interface ICameraManagement
    {
        void Init(Transform cameraTransform, Camera camera, IMazeJumper maze);
        void MoveLeft();
        void MoveRight();
        void MoveUp();
        void MoveDown();
        void ZoomIn();
        void ZoomOut();
    }
}
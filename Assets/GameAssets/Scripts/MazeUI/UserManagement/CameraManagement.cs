using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.UserManagement
{
    public class CameraManagement : ICameraManagement
    {
        private Transform _cameraTransform;
        private Camera _camera;

        public int speed;
        private float _currentSize;
        private IMazeJumper _maze;


        public CameraManagement()
        {
            speed = 10;
        }

        public void Init(Transform cameraTransform, Camera camera, IMazeJumper maze)
        {
            _cameraTransform = cameraTransform;
            _camera = camera;
            _currentSize = _camera.orthographicSize;
            _maze = maze;
        }

        public void MoveLeft()
        {
            if (_cameraTransform.localPosition.x > 0)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x - speed, existingPosition.y, existingPosition.z);
            }
        }

        public void MoveRight()
        {
            if (_cameraTransform.localPosition.x < 36 * _maze.Size.X)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x + speed, existingPosition.y, existingPosition.z);
            }
        }

        public void MoveUp()
        {
            if (_cameraTransform.localPosition.y < 36 * _maze.Size.Y)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x, existingPosition.y + speed, existingPosition.z);
            }
        }

        public void MoveDown()
        {
            if (_cameraTransform.localPosition.y > 0)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x, existingPosition.y - speed, existingPosition.z);
            }
        }

        public void ZoomIn()
        {
            if (_camera.orthographicSize > 20)
            {
                _currentSize = _camera.orthographicSize - 1;
                _camera.orthographicSize = _currentSize;
            }
            
        }

        public void ZoomOut()
        {
            if (_camera.orthographicSize < 800)
            {
                _currentSize = _camera.orthographicSize + 1;
                _camera.orthographicSize = _currentSize;
            }
        }
    }
}

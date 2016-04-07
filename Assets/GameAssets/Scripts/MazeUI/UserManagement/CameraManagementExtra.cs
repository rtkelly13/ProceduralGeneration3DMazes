using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.UserManagement
{
    public class CameraManagementExtra : ICameraManagement
    {
        private Transform _cameraTransform;
        private Camera _camera;

        public int speed;
        private float _currentSize = 1;
        private IMazeJumper _maze;


        public CameraManagementExtra()
        {
            speed = 5;
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
            var min = MinXY();
            if (_cameraTransform.localPosition.x > min)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x - speed, existingPosition.y, existingPosition.z);
            }
        }

        public void MoveRight()
        {
            var max = MaxX();
            if (_cameraTransform.localPosition.x < max)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x + speed, existingPosition.y, existingPosition.z);
            }
        }

        public void MoveUp()
        {
            var max = MaxY();
            if (_cameraTransform.localPosition.y < max)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x, existingPosition.y + speed, existingPosition.z);
            }
        }

        public void MoveDown()
        {
            var min = MinXY();
            if (_cameraTransform.localPosition.y > min)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x, existingPosition.y - speed, existingPosition.z);
            }
        }

        private int MinXY()
        {
            return (int)(_currentSize / 8 * 7);
        }

        private int MaxX()
        {
            return (36 * _maze.Size.X) - (int)(_currentSize / 8 * 7) - 10;
        }

        private int MaxY()
        {
            return (36 * _maze.Size.Y) - (int)(_currentSize / 8 * 7) - 10;
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

        private void RealignCamera()
        {
            while(_cameraTransform.localPosition.x < MinXY())
            {
                MoveRight();
            }
            while (_cameraTransform.localPosition.x > MaxX())
            {
                MoveLeft();
            }
            while (_cameraTransform.localPosition.y < MinXY())
            {
                MoveUp();
            }
            while (_cameraTransform.localPosition.y > MaxY())
            {
                MoveDown();
            }
        }
    }
}

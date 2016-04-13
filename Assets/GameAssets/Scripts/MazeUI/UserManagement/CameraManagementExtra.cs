using Assets.GameAssets.Scripts.Maze.Model;
using Assets.GameAssets.Scripts.MazeUI.ImageHandling;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.UserManagement
{
    public class CameraManagementExtra : ICameraManagement
    {
        private readonly ICellInformationProvider _cellInformation;
        private IMazeJumper _maze;

        private Transform _cameraTransform;
        private Camera _camera;

        private float _currentSize = 1;
        private int _speed;

        private int _screenWidth;
        private int _screenHeight;
        private float _aspectRatio;

        public CameraManagementExtra(ICellInformationProvider cellInformation)
        {
            _cellInformation = cellInformation;
            _speed = 10;
        }

        public void Init(Transform cameraTransform, Camera camera, IMazeJumper maze)
        {
            _cameraTransform = cameraTransform;
            _camera = camera;
            _currentSize = _camera.orthographicSize;
            _maze = maze;
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            _aspectRatio = (float)_screenWidth/_screenHeight;
        }

        public void MoveLeft()
        {
            var min = MinX();
            if (_cameraTransform.localPosition.x > min)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x - _speed, existingPosition.y, existingPosition.z);
            }
        }

        public void MoveRight()
        {
            var max = MaxX();
            if (_cameraTransform.localPosition.x < max)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x + _speed, existingPosition.y, existingPosition.z);
            }
        }

        public void MoveUp()
        {
            var max = MaxY();
            if (_cameraTransform.localPosition.y < max)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x, existingPosition.y + _speed, existingPosition.z);
            }
        }

        public void MoveDown()
        {
            var min = MinY();
            if (_cameraTransform.localPosition.y > min)
            {
                var existingPosition = _cameraTransform.localPosition;
                _cameraTransform.localPosition = new Vector3(existingPosition.x, existingPosition.y - _speed, existingPosition.z);
            }
        }

        private int MinX()
        {
            return 0;
        }

        private int MinY()
        {
            return 0;
        }

        private int MaxX()
        {
            return (_cellInformation.CellSize * _maze.Size.X);
        }

        private int MaxY()
        {
            return (_cellInformation.CellSize * _maze.Size.Y);
        }

        public void ZoomIn()
        {
            if (_camera.orthographicSize > 20)
            {
                _currentSize = _camera.orthographicSize - _speed;
                _camera.orthographicSize = _currentSize;
            }
        }


        public void ZoomOut()
        {
            if (_camera.orthographicSize < 1400)
            {
                _currentSize = _camera.orthographicSize + _speed;
                _camera.orthographicSize = _currentSize;
            }
        }

        private void RealignCamera()
        {
            while(_cameraTransform.localPosition.x < MinX())
            {
                MoveRight();
            }
            while (_cameraTransform.localPosition.x > MaxX())
            {
                MoveLeft();
            }
            while (_cameraTransform.localPosition.y < MinY())
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

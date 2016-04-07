using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.UserManagement
{
    public class InputHandler : IInputHandler
    {
        private ICameraManagement _cameraManagement;
        private InputHandlerOptions _options;

        public void Init(ICameraManagement cameraManagement, InputHandlerOptions options)
        {
            _options = options;
            _cameraManagement = cameraManagement;
        }

        public void HandleUpdate()
        {
            if (Input.GetKey(KeyCode.W))
            {
                _cameraManagement.MoveUp();
            }
            if (Input.GetKey(KeyCode.A))
            {
                _cameraManagement.MoveLeft();
            }
            if (Input.GetKey(KeyCode.S))
            {
                _cameraManagement.MoveDown();
            }
            if (Input.GetKey(KeyCode.D))
            {
                _cameraManagement.MoveRight();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                _options.MoveUp();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                _options.MoveDown();
            }
            if (Input.GetKey(KeyCode.Z))
            {
               _cameraManagement.ZoomIn();
            }
            if (Input.GetKey(KeyCode.X))
            {
                _cameraManagement.ZoomOut();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                _options.ToggleDeadEnds();
            }
        } 
    }
}

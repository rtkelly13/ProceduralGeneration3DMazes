namespace Assets.GameAssets.Scripts.MazeUI.UserManagement
{
    public interface IInputHandler
    {
        void HandleUpdate();
        void Init(ICameraManagement cameraManagement,InputHandlerOptions options);
    }
}
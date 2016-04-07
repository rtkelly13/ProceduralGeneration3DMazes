using System;

namespace Assets.GameAssets.Scripts.MazeUI.UserManagement
{
    public class InputHandlerOptions
    {
        public Action MoveUp { get; set; }
        public Action MoveDown { get; set; }
        public Action ToggleDeadEnds { get; set; }
    }
}
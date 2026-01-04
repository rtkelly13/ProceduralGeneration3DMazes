using Godot;
using System.Collections.Generic;

namespace ProceduralMaze.UI
{
    /// <summary>
    /// A modal dialog that displays all keyboard shortcuts organized by category.
    /// Uses CanvasLayer to render above all other UI elements.
    /// </summary>
    public partial class ShortcutsModal : CanvasLayer
    {
        private Control _control = null!;
        private ColorRect _dimmer = null!;
        private PanelContainer _panel = null!;
        private Button _closeButton = null!;

        public override void _Ready()
        {
            _control = GetNode<Control>("Control");
            _dimmer = GetNode<ColorRect>("Control/Dimmer");
            _panel = GetNode<PanelContainer>("Control/Panel");
            _closeButton = GetNode<Button>("Control/Panel/MarginContainer/VBoxContainer/CloseButton");
            
            _closeButton.Pressed += OnClosePressed;
            
            // Connect dimmer click to close (clicking outside panel)
            _dimmer.GuiInput += OnDimmerInput;

            // Start hidden
            Visible = false;
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;

            // Close on Escape or H key
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.Escape || keyEvent.Keycode == Key.H)
                {
                    Hide();
                    GetViewport().SetInputAsHandled();
                }
            }
        }

        private void OnDimmerInput(InputEvent @event)
        {
            // Close if clicking on the dimmer (outside the panel)
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                Hide();
                GetViewport().SetInputAsHandled();
            }
        }

        private void OnClosePressed()
        {
            Hide();
        }

        public new void Show()
        {
            Visible = true;
        }

        public new void Hide()
        {
            Visible = false;
        }

        public void Toggle()
        {
            Visible = !Visible;
        }

        /// <summary>
        /// Returns all shortcuts organized by category for building the UI.
        /// </summary>
        public static Dictionary<string, List<(string Key, string Description)>> GetShortcutsByCategory()
        {
            return new Dictionary<string, List<(string Key, string Description)>>
            {
                ["Navigation"] = new()
                {
                    ("W/A/S/D", "Pan view"),
                    ("Q / E", "Zoom out / in"),
                    ("Mouse Wheel", "Zoom"),
                    ("Z / X", "Previous / next level (3D mazes)")
                },
                ["Display"] = new()
                {
                    ("C", "Toggle hide dead ends"),
                    ("P", "Toggle shortest path overlay"),
                    ("G", "Toggle graph overlay"),
                    ("V", "Toggle graph view mode")
                },
                ["Graph View"] = new()
                {
                    ("N / B", "Next / previous alternative path"),
                    ("M", "Show all paths simultaneously"),
                    ("J", "Cycle junction detail level"),
                    ("T", "Toggle algorithm animation")
                },
                ["Animation"] = new()
                {
                    ("Space", "Play / pause"),
                    ("< / >", "Step backward / forward"),
                    ("+ / -", "Speed up / slow down"),
                    ("T", "Exit animation mode")
                },
                ["File"] = new()
                {
                    ("Ctrl+S", "Quick export maze"),
                    ("Ctrl+Shift+S", "Export maze as..."),
                    ("Ctrl+O", "Import maze")
                },
                ["General"] = new()
                {
                    ("R", "Regenerate maze"),
                    ("H", "Show/hide this help"),
                    ("Esc", "Return to menu")
                }
            };
        }
    }
}

using System.Collections.Generic;
using Godot;
using ProceduralMaze.Build;

namespace ProceduralMaze.UI
{
    /// <summary>
    /// About screen. Its job is to answer one question precisely: <em>which build is this?</em>
    /// </summary>
    /// <remarks>
    /// The commit is shown in full rather than abbreviated. The point of the screen is to compare
    /// what is being served against a workflow run or a <c>git rev-parse HEAD</c>, and a
    /// seven-character prefix makes that comparison weaker for no gain on a screen with room.
    ///
    /// A thin adapter, as this codebase requires of Godot types: what the build *is*, and how it
    /// renders as text, lives in <see cref="BuildInfo"/> where the NUnit suite covers it. What is
    /// left here is node wiring plus the two rows that genuinely need the engine.
    /// </remarks>
    public partial class AboutUI : Control
    {
        private Label _summaryLabel = null!;
        private VBoxContainer _rows = null!;
        private Button _backButton = null!;
        private Button _copyButton = null!;
        private Label _copyFeedback = null!;

        public override void _Ready()
        {
            _summaryLabel = GetNode<Label>("%SummaryLabel");
            _rows = GetNode<VBoxContainer>("%Rows");
            _backButton = GetNode<Button>("%BackButton");
            _copyButton = GetNode<Button>("%CopyButton");
            _copyFeedback = GetNode<Label>("%CopyFeedback");

            _backButton.Pressed += ReturnToMenu;
            _copyButton.Pressed += OnCopyPressed;
            _copyFeedback.Text = string.Empty;

            _summaryLabel.Text = CurrentBuild.Info.Summary;
            Populate();
        }

        public override void _Input(InputEvent @event)
        {
            // Escape goes back, matching every other screen in the app.
            if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
            {
                ReturnToMenu();
                GetViewport().SetInputAsHandled();
            }
        }

        /// <summary>
        /// Rows to display: build provenance from <see cref="BuildInfo"/>, then the runtime facts
        /// only the engine can answer.
        /// </summary>
        public static List<KeyValuePair<string, string>> DisplayRows(BuildInfo info)
        {
            var rows = new List<KeyValuePair<string, string>>(info.DisplayRows())
            {
                new("Engine", EngineVersion()),
                new("Platform", OS.GetName()),
            };
            return rows;
        }

        private static string EngineVersion() => (string)Engine.GetVersionInfo()["string"];

        private void ReturnToMenu() => GetTree().ChangeSceneToFile("res://scenes/menu.tscn");

        private void Populate()
        {
            foreach (var child in _rows.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var row in DisplayRows(CurrentBuild.Info))
            {
                _rows.AddChild(MakeRow(row.Key, row.Value));
            }
        }

        private static HBoxContainer MakeRow(string label, string value)
        {
            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 12);

            var name = new Label
            {
                Text = label,
                CustomMinimumSize = new Vector2(130, 0),
            };
            name.AddThemeColorOverride("font_color", new Color(0.66f, 0.70f, 0.80f));
            container.AddChild(name);

            var text = new Label
            {
                Text = value,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            container.AddChild(text);

            return container;
        }

        private void OnCopyPressed()
        {
            DisplayServer.ClipboardSet(CurrentBuild.Info.ToClipboardText() + "engine: " + EngineVersion() + "\n");
            _copyFeedback.Text = "Copied";
        }
    }
}

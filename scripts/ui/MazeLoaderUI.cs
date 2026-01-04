using Godot;
using ProceduralMaze.Autoload;

namespace ProceduralMaze.UI
{
    public partial class MazeLoaderUI : Control
    {
        private ProgressBar _progressBar = null!;
        private Label _statusLabel = null!;
        private Godot.Timer _timer = null!;
        private Button _backButton = null!;

        public override void _Ready()
        {
            _progressBar = GetNode<ProgressBar>("%ProgressBar");
            _statusLabel = GetNode<Label>("%StatusLabel");
            _timer = GetNode<Godot.Timer>("%Timer");
            _backButton = GetNode<Button>("%BackButton");

            _timer.Timeout += OnTimerTimeout;
            _backButton.Pressed += OnBackButtonPressed;
            _backButton.Visible = false;

            // Start the loading process after a short delay to allow UI to render
            _statusLabel.Text = "Initializing...";
            _progressBar.Value = 0;
            _timer.Start();
        }

        private void OnBackButtonPressed()
        {
            GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
        }

        private void OnTimerTimeout()
        {
            GenerateMaze();
        }

        private void GenerateMaze()
        {
            _statusLabel.Text = "Building maze model...";
            _progressBar.Value = 20;

            if (GameState.Instance == null)
            {
                ShowError("GameState not initialized");
                return;
            }

            try
            {
                _statusLabel.Text = "Generating maze...";
                _progressBar.Value = 40;

                // Reset any previous visualization state before generating new maze
                GameState.Instance.ResetVisualizationState();
                
                var result = GameState.Instance.GenerateMaze();

                _statusLabel.Text = "Maze generated successfully!";
                _progressBar.Value = 80;

                // Log some stats
                var heuristics = result.HeuristicsResults;
                GD.Print($"Maze generated: {result.MazeJumper.Size.X}x{result.MazeJumper.Size.Y}x{result.MazeJumper.Size.Z}");
                GD.Print($"Total cells: {heuristics.TotalCells}");
                GD.Print($"Shortest path length: {heuristics.ShortestPathResult.ShortestPath}");
                GD.Print($"Total time: {result.TotalTime.TotalMilliseconds}ms");

                _progressBar.Value = 100;
                _statusLabel.Text = "Loading maze view...";

                // Transition to maze scene
                GetTree().ChangeSceneToFile("res://scenes/maze.tscn");
            }
            catch (System.Exception ex)
            {
                ShowError(ex.Message);
                GD.PrintErr($"Maze generation failed: {ex}");
            }
        }

        private void ShowError(string message)
        {
            _statusLabel.Text = $"Error: {message}";
            _progressBar.Value = 0;
            _backButton.Visible = true;
        }
    }
}

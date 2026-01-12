using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.UI
{
    public partial class ComparisonController : Control
    {
        private OptionButton _algorithmAOption = null!;
        private OptionButton _algorithmBOption = null!;
        private CheckButton _heatmapCheck = null!;
        private Button _startButton = null!;
        private Button _resetButton = null!;
        private Button _backButton = null!;
        private Label _statusLabel = null!;
        
        private SubViewport _leftViewport = null!;
        private SubViewport _rightViewport = null!;
        private Node2D _heatmapA = null!;
        private Node2D _heatmapB = null!;
        
        private Label _metricsALabel = null!;
        private Label _metricsBLabel = null!;

        private MazeGenerationResults? _resultsA;
        private MazeGenerationResults? _resultsB;
        
        private int _currentStep = 0;
        private bool _isPlaying = false;
        private float _playbackTimer = 0f;
        private const float StepDelay = 0.02f;

        public override void _Ready()
        {
            // Path matches the polished VBox structure in tscn
            var controls = GetNode<HBoxContainer>("MainLayout/TopPanel/Margin/VBox/Controls");
            _algorithmAOption = controls.GetNode<OptionButton>("AlgorithmAOption");
            _algorithmBOption = controls.GetNode<OptionButton>("AlgorithmBOption");
            _heatmapCheck = controls.GetNode<CheckButton>("HeatmapCheck");
            _startButton = controls.GetNode<Button>("StartButton");
            _resetButton = controls.GetNode<Button>("ResetButton");
            _backButton = controls.GetNode<Button>("BackButton");
            _statusLabel = controls.GetNode<Label>("StatusLabel");
            
            _leftViewport = GetNode<SubViewport>("MainLayout/Split/LeftContainer/LeftViewport");
            _rightViewport = GetNode<SubViewport>("MainLayout/Split/RightContainer/RightViewport");
            _heatmapA = _leftViewport.GetNode<Node2D>("HeatmapA");
            _heatmapB = _rightViewport.GetNode<Node2D>("HeatmapB");
            
            _metricsALabel = GetNode<Label>("MainLayout/Split/LeftContainer/LeftMetricsPanel/Margin/MetricsALabel");
            _metricsBLabel = GetNode<Label>("MainLayout/Split/RightContainer/RightMetricsPanel/Margin/MetricsBLabel");

            _startButton.Pressed += OnStartButtonPressed;
            _resetButton.Pressed += Reset;
            _backButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
            _heatmapCheck.Toggled += (pressed) => UpdateVisualization();
            
            PopulateAlgorithmOptions();
        }

        public override void _Process(double delta)
        {
            if (!_isPlaying) return;

            _playbackTimer += (float)delta;
            if (_playbackTimer >= StepDelay)
            {
                _playbackTimer = 0f;
                StepForward();
            }
        }
        
        private void PopulateAlgorithmOptions()
        {
            foreach (var algo in Enum.GetValues(typeof(ProceduralMaze.Maze.Algorithm)))
            {
                if ((ProceduralMaze.Maze.Algorithm)algo == ProceduralMaze.Maze.Algorithm.None) continue;
                _algorithmAOption.AddItem(algo.ToString());
                _algorithmBOption.AddItem(algo.ToString());
            }
        }

        private void OnStartButtonPressed()
        {
            if (_resultsA == null)
            {
                StartComparison();
            }
            else
            {
                TogglePlayPause();
            }
        }

        public void StartComparison()
        {
            Reset();
            
            var algoA = (ProceduralMaze.Maze.Algorithm)Enum.Parse(typeof(ProceduralMaze.Maze.Algorithm), _algorithmAOption.Text);
            var algoB = (ProceduralMaze.Maze.Algorithm)Enum.Parse(typeof(ProceduralMaze.Maze.Algorithm), _algorithmBOption.Text);
            
            var settingsA = CreateSettings(algoA);
            var settingsB = CreateSettings(algoB);
            
            var services = GameState.Instance?.Services ?? new ServiceContainer();
            
            _resultsA = services.MazeGenerationFactory.GenerateMaze(settingsA);
            _resultsB = services.MazeGenerationFactory.GenerateMaze(settingsB);
            
            _isPlaying = true;
            _startButton.Text = "Pause";
            _statusLabel.Text = "Status: Running";
        }

        private MazeGenerationSettings CreateSettings(ProceduralMaze.Maze.Algorithm algo)
        {
            return new MazeGenerationSettings
            {
                Algorithm = algo,
                Size = new MazeSize { X = 30, Y = 20, Z = 1 },
                Option = MazeType.ArrayBidirectional,
                GrowingTreeSettings = new GrowingTreeSettings 
                { 
                    NewestWeight = 100,
                    OldestWeight = 0,
                    RandomWeight = 0
                }
            };
        }

        public void TogglePlayPause()
        {
            _isPlaying = !_isPlaying;
            _startButton.Text = _isPlaying ? "Pause" : "Resume";
            _statusLabel.Text = _isPlaying ? "Status: Running" : "Status: Paused";
        }

        public void StepForward()
        {
            _currentStep += 2;
            UpdateVisualization();
            
            int maxSteps = Math.Max(_resultsA?.DirectionsCarvedIn.Count ?? 0, _resultsB?.DirectionsCarvedIn.Count ?? 0);
            if (_currentStep >= maxSteps)
            {
                _isPlaying = false;
                _statusLabel.Text = "Status: Finished";
                _startButton.Text = "Finished";
                _startButton.Disabled = true;
            }
        }

        private void UpdateVisualization()
        {
            if (_resultsA != null) {
                UpdateMetrics(_metricsALabel, _resultsA, _currentStep);
                RenderHeatmap(_heatmapA, _resultsA, _currentStep);
            }
            if (_resultsB != null) {
                UpdateMetrics(_metricsBLabel, _resultsB, _currentStep);
                RenderHeatmap(_heatmapB, _resultsB, _currentStep);
            }
        }

        private void RenderHeatmap(Node2D parent, MazeGenerationResults results, int step)
        {
            parent.Visible = _heatmapCheck.ButtonPressed;
            if (!parent.Visible) return;

            // Clear old heatmap sprites
            foreach (var child in parent.GetChildren()) child.QueueFree();

            if (results.Heatmap == null || results.Heatmap.Count == 0) return;

            int cellSize = 20; 
            int maxVisits = results.Heatmap.Values.Max();
            
            var visitedPoints = new HashSet<MazePoint>();
            int limit = Math.Min(step, results.DirectionsCarvedIn.Count);
            
            for (int i = 0; i < limit; i++)
            {
                visitedPoints.Add(results.DirectionsCarvedIn[i].MazePoint);
            }
            
            if (limit > 0 && results.MazeJumper != null)
            {
                visitedPoints.Add(results.MazeJumper.StartPoint);
            }

            foreach (var kvp in results.Heatmap)
            {
                var point = kvp.Key;
                if (!visitedPoints.Contains(point)) continue;
                
                var visits = kvp.Value;
                
                var rect = new ColorRect();
                rect.Size = new Vector2(cellSize - 2, cellSize - 2);
                rect.Position = new Vector2(point.X * cellSize + 1, point.Y * cellSize + 1);
                
                float intensity = (float)visits / maxVisits;
                rect.Color = new Color(intensity, 0.2f, 1.0f - intensity, 0.5f);
                parent.AddChild(rect);
            }
        }

        private void UpdateMetrics(Label label, MazeGenerationResults results, int step)
        {
            var metrics = results.Metrics;
            double progress = Math.Min(1.0, (double)step / Math.Max(1, results.DirectionsCarvedIn.Count));
            
            label.Text = $"Steps: {Math.Min(step, results.DirectionsCarvedIn.Count)}\n" +
                         $"Dead Ends: {(int)(metrics.DeadEnds * progress)}\n" +
                         $"Branching: {metrics.BranchingFactor:F2}\n" +
                         $"Gen Time: {results.GenerationTime.TotalMilliseconds:F0}ms";
        }

        public void Reset()
        {
            _isPlaying = false;
            _currentStep = 0;
            _playbackTimer = 0f;
            _resultsA = null;
            _resultsB = null;
            _startButton.Text = "Start";
            _startButton.Disabled = false;
            _statusLabel.Text = "Status: Ready";
            _metricsALabel.Text = "Steps: 0\nDead Ends: 0\nBranching: 0\nGen Time: 0ms";
            _metricsBLabel.Text = "Steps: 0\nDead Ends: 0\nBranching: 0\nGen Time: 0ms";
            
            if (_heatmapA != null) foreach (var child in _heatmapA.GetChildren()) child.QueueFree();
            if (_heatmapB != null) foreach (var child in _heatmapB.GetChildren()) child.QueueFree();
        }
    }
}

using Godot;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.UI
{
    public partial class MenuUI : Control
    {
        // Left panel controls
        private OptionButton _algorithmOption = null!;
        private HSlider _xSlider = null!;
        private HSlider _ySlider = null!;
        private HSlider _zSlider = null!;
        private Label _xValue = null!;
        private Label _yValue = null!;
        private Label _zValue = null!;
        private Button _xReset = null!;
        private Button _yReset = null!;
        private Button _zReset = null!;
        private OptionButton _mazeTypeOption = null!;
        private CheckButton _doorsAtEdgeCheck = null!;
        private SpinBox _wallRemovalSpinBox = null!;
        private Button _wallRemovalPreset0 = null!;
        private Button _wallRemovalPreset1 = null!;
        private Button _wallRemovalPreset2 = null!;
        private Button _wallRemovalPreset3 = null!;
        private Button _wallRemovalPreset5 = null!;
        private Button _wallRemovalPreset8 = null!;
        private Button _wallRemovalPreset11 = null!;
        private Button _wallRemovalPreset15 = null!;
        private OptionButton _agentOption = null!;

        // Right panel controls (algorithm settings)
        private HSlider _newestSlider = null!;
        private HSlider _oldestSlider = null!;
        private HSlider _randomSlider = null!;
        private Label _newestValue = null!;
        private Label _oldestValue = null!;
        private Label _randomValue = null!;

        // Bottom controls
        private Label _validationLabel = null!;
        private Button _generateButton = null!;
        private Button _randomizeOptionsButton = null!;
        private Button _randomizeWeightsButton = null!;
        private Button _randomizeAllButton = null!;
        private Button _resetButton = null!;
        private Button _importButton = null!;

        // Import UI
        private FileDialog _importDialog = null!;
        private ToastNotification _toast = null!;

        // Random number generator
        private readonly System.Random _random = new();

        public override void _Ready()
        {
            // Get references to all controls
            _algorithmOption = GetNode<OptionButton>("%AlgorithmOption");
            _xSlider = GetNode<HSlider>("%XSlider");
            _ySlider = GetNode<HSlider>("%YSlider");
            _zSlider = GetNode<HSlider>("%ZSlider");
            _xValue = GetNode<Label>("%XValue");
            _yValue = GetNode<Label>("%YValue");
            _zValue = GetNode<Label>("%ZValue");
            _xReset = GetNode<Button>("%XReset");
            _yReset = GetNode<Button>("%YReset");
            _zReset = GetNode<Button>("%ZReset");
            _mazeTypeOption = GetNode<OptionButton>("%MazeTypeOption");
            _doorsAtEdgeCheck = GetNode<CheckButton>("%DoorsAtEdgeCheck");
            _wallRemovalSpinBox = GetNode<SpinBox>("%WallRemovalSpinBox");
            _wallRemovalPreset0 = GetNode<Button>("%Preset0");
            _wallRemovalPreset1 = GetNode<Button>("%Preset1");
            _wallRemovalPreset2 = GetNode<Button>("%Preset2");
            _wallRemovalPreset3 = GetNode<Button>("%Preset3");
            _wallRemovalPreset5 = GetNode<Button>("%Preset5");
            _wallRemovalPreset8 = GetNode<Button>("%Preset8");
            _wallRemovalPreset11 = GetNode<Button>("%Preset11");
            _wallRemovalPreset15 = GetNode<Button>("%Preset15");
            _agentOption = GetNode<OptionButton>("%AgentOption");

            _newestSlider = GetNode<HSlider>("%NewestSlider");
            _oldestSlider = GetNode<HSlider>("%OldestSlider");
            _randomSlider = GetNode<HSlider>("%RandomSlider");
            _newestValue = GetNode<Label>("%NewestValue");
            _oldestValue = GetNode<Label>("%OldestValue");
            _randomValue = GetNode<Label>("%RandomValue");

            _validationLabel = GetNode<Label>("%ValidationLabel");
            _generateButton = GetNode<Button>("%GenerateButton");
            _randomizeOptionsButton = GetNode<Button>("%RandomizeOptionsButton");
            _randomizeWeightsButton = GetNode<Button>("%RandomizeWeightsButton");
            _randomizeAllButton = GetNode<Button>("%RandomizeAllButton");
            _resetButton = GetNode<Button>("%ResetButton");
            _importButton = GetNode<Button>("%ImportButton");

            // Import dialog and toast
            _importDialog = GetNode<FileDialog>("%ImportDialog");
            _toast = GetNode<ToastNotification>("%Toast");

            // Set up default import path
            var defaultDir = MazeImportExport.GetDefaultExportDirectory();
            _importDialog.CurrentDir = defaultDir;

            // Handle web platform (import not available)
            if (MazeImportExport.IsWebPlatform())
            {
                _importButton.Disabled = true;
                _importButton.TooltipText = "Import is not available on web platform";
            }

            // Setup dropdowns
            SetupAlgorithmDropdown();
            SetupMazeTypeDropdown();
            SetupAgentDropdown();
            SetupTooltips();

            // Connect signals
            _algorithmOption.ItemSelected += OnAlgorithmSelected;
            _xSlider.ValueChanged += v => OnSizeSliderChanged(v, val => GameState.Instance!.Settings.Size.X = val, _xValue);
            _ySlider.ValueChanged += v => OnSizeSliderChanged(v, val => GameState.Instance!.Settings.Size.Y = val, _yValue);
            _zSlider.ValueChanged += v => OnSizeSliderChanged(v, val => GameState.Instance!.Settings.Size.Z = val, _zValue);
            _xReset.Pressed += () => ResetAxisValue(_xSlider, 20);
            _yReset.Pressed += () => ResetAxisValue(_ySlider, 20);
            _zReset.Pressed += () => ResetAxisValue(_zSlider, 1);
            _mazeTypeOption.ItemSelected += OnMazeTypeSelected;
            _doorsAtEdgeCheck.Toggled += OnDoorsAtEdgeToggled;
            _wallRemovalSpinBox.ValueChanged += OnWallRemovalChanged;
            _wallRemovalPreset0.Pressed += () => SetWallRemovalPercent(0);
            _wallRemovalPreset1.Pressed += () => SetWallRemovalPercent(1);
            _wallRemovalPreset2.Pressed += () => SetWallRemovalPercent(2);
            _wallRemovalPreset3.Pressed += () => SetWallRemovalPercent(3);
            _wallRemovalPreset5.Pressed += () => SetWallRemovalPercent(5);
            _wallRemovalPreset8.Pressed += () => SetWallRemovalPercent(8);
            _wallRemovalPreset11.Pressed += () => SetWallRemovalPercent(11);
            _wallRemovalPreset15.Pressed += () => SetWallRemovalPercent(15);
            _agentOption.ItemSelected += OnAgentSelected;

            _newestSlider.ValueChanged += v => OnWeightSliderChanged(v, val => GameState.Instance!.Settings.GrowingTreeSettings!.NewestWeight = val, _newestValue);
            _oldestSlider.ValueChanged += v => OnWeightSliderChanged(v, val => GameState.Instance!.Settings.GrowingTreeSettings!.OldestWeight = val, _oldestValue);
            _randomSlider.ValueChanged += v => OnWeightSliderChanged(v, val => GameState.Instance!.Settings.GrowingTreeSettings!.RandomWeight = val, _randomValue);

            _generateButton.Pressed += OnGeneratePressed;
            _randomizeOptionsButton.Pressed += OnRandomizeOptionsPressed;
            _randomizeWeightsButton.Pressed += OnRandomizeWeightsPressed;
            _randomizeAllButton.Pressed += OnRandomizeAllPressed;
            _resetButton.Pressed += OnResetPressed;
            _importButton.Pressed += OnImportPressed;
            _importDialog.FileSelected += OnImportFileSelected;

            // Load current settings
            LoadCurrentSettings();
            Validate();
        }

        private void SetupAlgorithmDropdown()
        {
            _algorithmOption.Clear();
            _algorithmOption.AddItem("Growing Tree Algorithm", (int)Algorithm.GrowingTreeAlgorithm);
            _algorithmOption.AddItem("Recursive Backtracker", (int)Algorithm.RecursiveBacktrackerAlgorithm);
            _algorithmOption.AddItem("Binary Tree Algorithm", (int)Algorithm.BinaryTreeAlgorithm);
        }

        private void SetupMazeTypeDropdown()
        {
            _mazeTypeOption.Clear();
            _mazeTypeOption.AddItem("Array Bidirectional", (int)MazeType.ArrayBidirectional);
            _mazeTypeOption.AddItem("Array Unidirectional", (int)MazeType.ArrayUnidirectional);
            _mazeTypeOption.AddItem("Dictionary", (int)MazeType.Dictionary);
        }

        private void SetupAgentDropdown()
        {
            _agentOption.Clear();
            _agentOption.AddItem("None", (int)AgentType.None);
            _agentOption.AddItem("Random Agent", (int)AgentType.Random);
            _agentOption.AddItem("Perfect Agent", (int)AgentType.Perfect);
        }

        private void SetupTooltips()
        {
            // Algorithm dropdown
            _algorithmOption.TooltipText = "Growing Tree: Configurable via weights below.\n" +
                "Recursive Backtracker: Depth-first, creates long winding passages.\n" +
                "Binary Tree: Fast but creates diagonal bias.";

            // Size sliders
            _xSlider.TooltipText = "Maze width (X dimension)";
            _ySlider.TooltipText = "Maze depth (Y dimension)";
            _zSlider.TooltipText = "Maze levels (Z dimension). Values > 1 create a multi-floor 3D maze with stairs.";

            // Maze type dropdown
            _mazeTypeOption.TooltipText = "Internal data structure for maze storage.\n" +
                "Array Bidirectional: Stores connections in both directions (recommended).\n" +
                "Array Unidirectional: One-way storage, uses less memory.\n" +
                "Dictionary: Sparse storage, efficient for mazes with many walls.";

            // Doors at edge
            _doorsAtEdgeCheck.TooltipText = "When enabled, start and end points are placed on maze boundaries.";

            // Wall removal
            _wallRemovalSpinBox.TooltipText = "Percentage of removable internal walls to remove after generation.\n" +
                "0% = Perfect maze with single solution.\n" +
                "Higher values create loops and multiple paths.\n" +
                "Use arrows or type a value. Up/Down keys work when focused.";

            // Agent dropdown
            _agentOption.TooltipText = "None: No simulation.\n" +
                "Random Agent: Explores randomly, avoids immediate backtracking.\n" +
                "Perfect Agent: Finds optimal path using depth-first search.";

            // Growing Tree weight sliders
            _newestSlider.TooltipText = "Weight for picking the newest cell (most recently added).\n" +
                "High values = Recursive Backtracker behavior with long, winding passages.";
            _oldestSlider.TooltipText = "Weight for picking the oldest cell (first added).\n" +
                "High values = Breadth-first behavior with shorter, more uniform passages.";
            _randomSlider.TooltipText = "Weight for picking a random cell.\n" +
                "High values = Prim's algorithm behavior with more branching.";
        }

        private void LoadCurrentSettings()
        {
            var settings = GameState.Instance?.Settings;
            if (settings == null) return;

            // Set dropdowns
            SelectOptionById(_algorithmOption, (int)settings.Algorithm);
            SelectOptionById(_mazeTypeOption, (int)settings.Option);
            SelectOptionById(_agentOption, (int)settings.AgentType);

            // Set size sliders
            _xSlider.Value = settings.Size.X;
            _ySlider.Value = settings.Size.Y;
            _zSlider.Value = settings.Size.Z;
            _xValue.Text = settings.Size.X.ToString();
            _yValue.Text = settings.Size.Y.ToString();
            _zValue.Text = settings.Size.Z.ToString();

            // Set doors at edge
            _doorsAtEdgeCheck.ButtonPressed = settings.DoorsAtEdge;

            // Set wall removal
            _wallRemovalSpinBox.Value = settings.WallRemovalPercent;

            // Set algorithm settings
            if (settings.GrowingTreeSettings != null)
            {
                _newestSlider.Value = settings.GrowingTreeSettings.NewestWeight;
                _oldestSlider.Value = settings.GrowingTreeSettings.OldestWeight;
                _randomSlider.Value = settings.GrowingTreeSettings.RandomWeight;
                _newestValue.Text = settings.GrowingTreeSettings.NewestWeight.ToString();
                _oldestValue.Text = settings.GrowingTreeSettings.OldestWeight.ToString();
                _randomValue.Text = settings.GrowingTreeSettings.RandomWeight.ToString();
            }
        }

        private static void SelectOptionById(OptionButton optionButton, int id)
        {
            for (int i = 0; i < optionButton.ItemCount; i++)
            {
                if (optionButton.GetItemId(i) == id)
                {
                    optionButton.Select(i);
                    return;
                }
            }
        }

        private void OnAlgorithmSelected(long index)
        {
            if (GameState.Instance == null) return;
            GameState.Instance.Settings.Algorithm = (Algorithm)_algorithmOption.GetItemId((int)index);
            Validate();
        }

        private void OnSizeSliderChanged(double value, System.Action<int> setter, Label label)
        {
            if (GameState.Instance == null) return;
            setter((int)value);
            label.Text = ((int)value).ToString();
            Validate();
        }

        private void OnWeightSliderChanged(double value, System.Action<int> setter, Label label)
        {
            if (GameState.Instance?.Settings.GrowingTreeSettings == null) return;
            setter((int)value);
            label.Text = ((int)value).ToString();
        }

        private void OnMazeTypeSelected(long index)
        {
            if (GameState.Instance == null) return;
            GameState.Instance.Settings.Option = (MazeType)_mazeTypeOption.GetItemId((int)index);
            Validate();
        }

        private void OnDoorsAtEdgeToggled(bool pressed)
        {
            if (GameState.Instance == null) return;
            GameState.Instance.Settings.DoorsAtEdge = pressed;
            Validate();
        }

        private void OnWallRemovalChanged(double value)
        {
            if (GameState.Instance == null) return;
            GameState.Instance.Settings.WallRemovalPercent = value;
            Validate();
        }

        private void SetWallRemovalPercent(double percent)
        {
            _wallRemovalSpinBox.Value = percent;
            // The SpinBox's ValueChanged event will handle the rest
        }

        private void ResetAxisValue(HSlider slider, int defaultValue)
        {
            slider.Value = defaultValue;
            // The slider's ValueChanged event will handle updating the label and settings
        }

        private void OnAgentSelected(long index)
        {
            if (GameState.Instance == null) return;
            GameState.Instance.Settings.AgentType = (AgentType)_agentOption.GetItemId((int)index);
            Validate();
        }

        private void Validate()
        {
            var settings = GameState.Instance?.Settings;
            if (settings == null)
            {
                _validationLabel.Text = "GameState not initialized";
                _generateButton.Disabled = true;
                return;
            }

            // Validate settings
            if (settings.Algorithm == Algorithm.None)
            {
                _validationLabel.Text = "Please select an algorithm";
                _generateButton.Disabled = true;
                return;
            }

            if (settings.Option == MazeType.None)
            {
                _validationLabel.Text = "Please select a maze type";
                _generateButton.Disabled = true;
                return;
            }

            if (settings.Size.X < 1 || settings.Size.Y < 1 || settings.Size.Z < 1)
            {
                _validationLabel.Text = "Maze size must be at least 1 in all dimensions";
                _generateButton.Disabled = true;
                return;
            }

            // Valid
            _validationLabel.Text = "";
            _generateButton.Disabled = false;
        }

        private void OnGeneratePressed()
        {
            GetTree().ChangeSceneToFile("res://scenes/maze_loader.tscn");
        }

        private void OnRandomizeOptionsPressed()
        {
            if (GameState.Instance == null) return;
            var settings = GameState.Instance.Settings;

            // Randomize algorithm
            var algorithms = new[] { Algorithm.GrowingTreeAlgorithm, Algorithm.RecursiveBacktrackerAlgorithm, Algorithm.BinaryTreeAlgorithm };
            settings.Algorithm = algorithms[_random.Next(algorithms.Length)];

            // Randomize size (reasonable range: 5-50)
            settings.Size.X = _random.Next(5, 51);
            settings.Size.Y = _random.Next(5, 51);
            settings.Size.Z = _random.Next(1, 6); // 1-5 levels

            // Randomize maze type
            var mazeTypes = new[] { MazeType.ArrayBidirectional, MazeType.Dictionary };
            settings.Option = mazeTypes[_random.Next(mazeTypes.Length)];

            // Randomize doors at edge
            settings.DoorsAtEdge = _random.Next(2) == 1;

            // Randomize wall removal (0-15% in 0.1 increments)
            settings.WallRemovalPercent = System.Math.Round(_random.NextDouble() * 15.0, 1);

            // Randomize agent
            var agentTypes = new[] { AgentType.None, AgentType.Random, AgentType.Perfect };
            settings.AgentType = agentTypes[_random.Next(agentTypes.Length)];

            LoadCurrentSettings();
            Validate();
        }

        private void OnRandomizeWeightsPressed()
        {
            if (GameState.Instance?.Settings.GrowingTreeSettings == null) return;
            var settings = GameState.Instance.Settings.GrowingTreeSettings;

            settings.NewestWeight = _random.Next(0, 101);
            settings.OldestWeight = _random.Next(0, 101);
            settings.RandomWeight = _random.Next(0, 101);

            LoadCurrentSettings();
        }

        private void OnRandomizeAllPressed()
        {
            OnRandomizeOptionsPressed();
            OnRandomizeWeightsPressed();
        }

        private void OnResetPressed()
        {
            if (GameState.Instance == null) return;

            // Reset to default settings
            GameState.Instance.Settings = new MazeGenerationSettings
            {
                Size = new MazeSize { X = 20, Y = 20, Z = 1 },
                Algorithm = Algorithm.GrowingTreeAlgorithm,
                Option = MazeType.ArrayBidirectional,
                DoorsAtEdge = false,
                WallRemovalPercent = 0,
                AgentType = AgentType.None,
                GrowingTreeSettings = new GrowingTreeSettings
                {
                    NewestWeight = 100,
                    OldestWeight = 0,
                    RandomWeight = 0
                }
            };

            LoadCurrentSettings();
            Validate();
        }

        #region Import

        private void OnImportPressed()
        {
            if (MazeImportExport.IsWebPlatform())
            {
                _toast.ShowError("Import is not available on web platform");
                return;
            }

            _importDialog.PopupCentered();
        }

        private void OnImportFileSelected(string path)
        {
            var gameState = GameState.Instance;
            if (gameState == null)
            {
                _toast.ShowError("Game state not available");
                return;
            }

            var result = MazeImportExport.Import(
                path,
                gameState.Services.MazeDeserializer,
                gameState.Services.MazeValidator);

            if (result.Success && result.Model != null)
            {
                // Load the imported maze and transition to maze view
                gameState.LoadImportedMaze(result.Model);
                GetTree().ChangeSceneToFile("res://scenes/maze.tscn");
            }
            else
            {
                _toast.ShowError(result.ErrorMessage ?? "Import failed", result.TechnicalDetails);
            }
        }

        #endregion
    }
}

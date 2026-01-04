using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.UI
{
    public partial class MazeMain : Control
    {
        // UI references
        private Node2D _mazeGrid = null!;
        private Label _mazeInfoLabel = null!;
        private Label _levelLabel = null!;
        private Label _statsLabel = null!;
        private CheckButton _deadEndsCheck = null!;
        private CheckButton _showPathCheck = null!;
        private CheckButton _showGraphCheck = null!;
        private CheckButton _showGraphViewCheck = null!;
        private PanelContainer _bottomPanel = null!;
        private Button _helpButton = null!;

        // Graph view UI elements
        private PanelContainer _legendPanel = null!;
        private PanelContainer _graphStatsPanel = null!;
        private Label _nodesLabel = null!;
        private Label _edgesLabel = null!;
        private Label _pathLengthLabel = null!;
        private Label _junctionsLabel = null!;
        private Label _pathIndexLabel = null!;

        // Import/Export UI elements
        private FileDialog _exportDialog = null!;
        private FileDialog _importDialog = null!;
        private ToastNotification _toast = null!;
        private Button _importButton = null!;
        private Button _exportButton = null!;
        private ShortcutsModal _shortcutsModal = null!;

        // Sprite textures (loaded dynamically)
        private Dictionary<string, Texture2D> _sprites = new();

        // Camera/pan state
        private Vector2 _panOffset = Vector2.Zero;
        private const float PanSpeed = 500f;
        private const float ZoomSpeed = 0.1f;
        private float _zoom = 1.0f;

        // Cell size for rendering
        private const int CellSize = 32;
        private const float CellSpriteSize = 240f; // Cell sprites are 240x240
        private const float MarkerSpriteSize = 566f; // Marker sprites are ~566x503 (use larger dimension)
        private const float StairSpriteSize = 600f; // Stair sprites are 600x498
        private float _cellScale;
        private float _markerScale;
        private float _stairScale;

        // Graph view renderer
        private GraphViewRenderer _graphViewRenderer = null!;

        public override void _Ready()
        {
            _mazeGrid = GetNode<Node2D>("%MazeGrid");
            _mazeInfoLabel = GetNode<Label>("%MazeInfoLabel");
            _levelLabel = GetNode<Label>("%LevelLabel");
            _statsLabel = GetNode<Label>("%StatsLabel");
            _deadEndsCheck = GetNode<CheckButton>("%DeadEndsCheck");
            _showPathCheck = GetNode<CheckButton>("%ShowPathCheck");
            _showGraphCheck = GetNode<CheckButton>("%ShowGraphCheck");
            _showGraphViewCheck = GetNode<CheckButton>("%ShowGraphViewCheck");
            _bottomPanel = GetNode<PanelContainer>("%BottomPanel");
            _helpButton = GetNode<Button>("%HelpButton");

            // Graph view UI elements
            _legendPanel = GetNode<PanelContainer>("%LegendPanel");
            _graphStatsPanel = GetNode<PanelContainer>("%GraphStatsPanel");
            _nodesLabel = GetNode<Label>("%NodesLabel");
            _edgesLabel = GetNode<Label>("%EdgesLabel");
            _pathLengthLabel = GetNode<Label>("%PathLengthLabel");
            _junctionsLabel = GetNode<Label>("%JunctionsLabel");
            
            // Path index label (may not exist yet in scene, create dynamically if needed)
            _pathIndexLabel = GetNodeOrNull<Label>("%PathIndexLabel") ?? CreatePathIndexLabel();

            // Import/Export dialogs and toast
            _exportDialog = GetNode<FileDialog>("%ExportDialog");
            _importDialog = GetNode<FileDialog>("%ImportDialog");
            _toast = GetNode<ToastNotification>("%Toast");
            _importButton = GetNode<Button>("%ImportButton");
            _exportButton = GetNode<Button>("%ExportButton");
            _shortcutsModal = GetNode<ShortcutsModal>("%ShortcutsModal");

            // Connect dialog signals
            _exportDialog.FileSelected += OnExportFileSelected;
            _importDialog.FileSelected += OnImportFileSelected;

            // Connect button signals
            _importButton.Pressed += ShowImportDialog;
            _exportButton.Pressed += ShowExportDialog;

            // Hide import/export buttons on web platform
            if (MazeImportExport.IsWebPlatform())
            {
                _importButton.Visible = false;
                _exportButton.Visible = false;
            }

            // Connect help button
            _helpButton.Pressed += () => _shortcutsModal.Toggle();

            // Set up default paths for dialogs
            var defaultDir = MazeImportExport.GetDefaultExportDirectory();
            _exportDialog.CurrentDir = defaultDir;
            _importDialog.CurrentDir = defaultDir;

            _deadEndsCheck.Toggled += OnDeadEndsToggled;
            _showPathCheck.Toggled += OnShowPathToggled;
            _showGraphCheck.Toggled += OnShowGraphToggled;
            _showGraphViewCheck.Toggled += OnShowGraphViewToggled;
            
            // Calculate scale to fit sprites into cell size (slight overlap to eliminate gaps)
            _cellScale = (CellSize / CellSpriteSize) * 1.05f;
            // Markers should be about 80% of cell size
            _markerScale = (CellSize * 0.8f) / MarkerSpriteSize;
            // Stairs should be about 35% of cell size, positioned in corners
            _stairScale = (CellSize * 0.35f) / StairSpriteSize;

            // Initialize graph view renderer
            _graphViewRenderer = new GraphViewRenderer(_mazeGrid, FitGraphViewToScreen);

            LoadSprites();
            SyncCheckboxStatesFromGameState();
            RenderMaze(centerView: true);
            UpdateUI();
        }

        /// <summary>
        /// Syncs checkbox states with GameState values on scene load.
        /// This ensures UI reflects the current state after scene transitions.
        /// </summary>
        private void SyncCheckboxStatesFromGameState()
        {
            if (GameState.Instance == null) return;

            _deadEndsCheck.ButtonPressed = GameState.Instance.HideDeadEnds;
            _showPathCheck.ButtonPressed = GameState.Instance.ShowPath;
            _showGraphCheck.ButtonPressed = GameState.Instance.ShowGraph;
            _showGraphViewCheck.ButtonPressed = GameState.Instance.ShowGraphView;
        }

        private void LoadSprites()
        {
            string[] spriteNames = {
                // Cell backgrounds
                "Back", "Cover", "Forward", "ForwardBack",
                "Left", "LeftBack", "LeftForward", "LeftForwardBack",
                "LeftRight", "LeftRightBack", "LeftRightForward", "LeftRightForwardBack",
                "Right", "RightBack", "RightForward", "RightForwardBack",
                "Uncover", "White",
                // Stair overlays
                "DownStairs", "UpStairs", "UpDownStairs", "Staircase", "UpDown",
                // Marker circles
                "blueCircle", "greenCircle", "greenStartCircle",
                "lightBlueCircle", "lightGreenCircle", "orangeCircle",
                "purpleCircle", "redCircle", "redEndCircle", "yellowCircle",
                // Lines
                "fullLineBlue", "fullLineGreen", "fullLineOrange", "fullLineRed", "fullLineYellow",
                "halfLineBlue", "halfLineGreen", "halfLineOrange", "halfLineRed", "halfLineYellow"
            };

            foreach (var name in spriteNames)
            {
                var path = $"res://resources/sprites/{name}.png";
                if (ResourceLoader.Exists(path))
                {
                    _sprites[name] = GD.Load<Texture2D>(path);
                }
            }
        }

        public override void _Process(double delta)
        {
            HandleInput((float)delta);
            
            // Update animation if playing
            UpdateAnimation((float)delta);
        }

        private void UpdateAnimation(float delta)
        {
            var gameState = GameState.Instance;
            if (gameState == null || !gameState.IsAnimationMode) return;
            
            var controller = gameState.AnimationController;
            if (controller == null) return;
            
            var previousStep = controller.CurrentStepIndex;
            var previousState = controller.State;
            controller.Update(delta);
            
            // If the step changed or state changed (e.g., finished), re-render
            if (controller.CurrentStepIndex != previousStep || controller.State != previousState)
            {
                RenderMaze();
                UpdateAnimationUI();
            }
        }

        private void HandleInput(float delta)
        {
            // Camera panning with WASD/Arrow keys
            // Pressing up/W moves the view up (content moves down, positive Y)
            Vector2 panInput = Vector2.Zero;
            if (Input.IsActionPressed("camera_up")) panInput.Y += 1;
            if (Input.IsActionPressed("camera_down")) panInput.Y -= 1;
            if (Input.IsActionPressed("camera_left")) panInput.X += 1;
            if (Input.IsActionPressed("camera_right")) panInput.X -= 1;

            if (panInput != Vector2.Zero)
            {
                _panOffset += panInput * PanSpeed * delta;
                _mazeGrid.Position = _panOffset;
            }

            // Level navigation
            if (Input.IsActionJustPressed("move_up_level"))
            {
                GameState.Instance?.NextLevel();
                RenderMaze();
                UpdateUI();
            }
            if (Input.IsActionJustPressed("move_down_level"))
            {
                GameState.Instance?.PreviousLevel();
                RenderMaze();
                UpdateUI();
            }

            // Toggle dead ends
            if (Input.IsActionJustPressed("toggle_dead_ends"))
            {
                if (GameState.Instance != null)
                {
                    GameState.Instance.HideDeadEnds = !GameState.Instance.HideDeadEnds;
                    _deadEndsCheck.ButtonPressed = GameState.Instance.HideDeadEnds;
                    RenderMaze();
                }
            }

            // Toggle path display
            if (Input.IsActionJustPressed("toggle_path"))
            {
                SetShowPath(!GameState.Instance?.ShowPath ?? false);
            }

            // Toggle graph display
            if (Input.IsActionJustPressed("toggle_graph"))
            {
                SetShowGraph(!GameState.Instance?.ShowGraph ?? false);
            }

            // Toggle graph view mode
            if (Input.IsActionJustPressed("toggle_graph_view"))
            {
                SetShowGraphView(!GameState.Instance?.ShowGraphView ?? false);
            }

            // Toggle help/controls panel (only open, closing is handled by the modal)
            if (Input.IsActionJustPressed("toggle_help") && !_shortcutsModal.Visible)
            {
                _shortcutsModal.Show();
            }

            // Alternative path navigation (graph view only)
            if (GameState.Instance?.ShowGraphView == true)
            {
                if (Input.IsActionJustPressed("next_path"))
                {
                    CycleToNextPath();
                }
                if (Input.IsActionJustPressed("previous_path"))
                {
                    CycleToPreviousPath();
                }
                if (Input.IsActionJustPressed("toggle_all_paths"))
                {
                    ToggleAllPathsView();
                }
                
                // Animation controls
                if (Input.IsActionJustPressed("toggle_animation"))
                {
                    ToggleAnimationMode();
                }
                
                if (GameState.Instance.IsAnimationMode)
                {
                    if (Input.IsActionJustPressed("animation_play_pause"))
                    {
                        GameState.Instance.AnimationController?.TogglePlayPause();
                        UpdateAnimationUI();
                    }
                    if (Input.IsActionJustPressed("animation_step_forward"))
                    {
                        GameState.Instance.AnimationController?.StepForward();
                        RenderMaze();
                        UpdateAnimationUI();
                    }
                    if (Input.IsActionJustPressed("animation_step_backward"))
                    {
                        GameState.Instance.AnimationController?.StepBackward();
                        RenderMaze();
                        UpdateAnimationUI();
                    }
                    if (Input.IsActionJustPressed("animation_speed_up"))
                    {
                        GameState.Instance.AnimationController?.SpeedUp();
                        UpdateAnimationUI();
                    }
                    if (Input.IsActionJustPressed("animation_speed_down"))
                    {
                        GameState.Instance.AnimationController?.SlowDown();
                        UpdateAnimationUI();
                    }
                }
                
                // Decision point toggle (J key)
                if (Input.IsActionJustPressed("toggle_decision_points"))
                {
                    CycleDecisionPointLevel();
                }
            }

            // Regenerate
            if (Input.IsActionJustPressed("regenerate"))
            {
                GetTree().ChangeSceneToFile("res://scenes/maze_loader.tscn");
            }

            // Return to menu
            if (Input.IsActionJustPressed("return_to_menu"))
            {
                GetTree().ChangeSceneToFile("res://scenes/menu.tscn");
            }

            // Export/Import (not available on web)
            if (!MazeImportExport.IsWebPlatform())
            {
                if (Input.IsActionJustPressed("export_maze"))
                {
                    QuickExport();
                }
                if (Input.IsActionJustPressed("export_maze_as"))
                {
                    ShowExportDialog();
                }
                if (Input.IsActionJustPressed("import_maze"))
                {
                    ShowImportDialog();
                }
            }

            // Zoom in/out with keyboard (E/Q)
            if (Input.IsActionPressed("zoom_in"))
            {
                ApplyZoom(ZoomSpeed * delta * 5f);
            }
            if (Input.IsActionPressed("zoom_out"))
            {
                ApplyZoom(-ZoomSpeed * delta * 5f);
            }
        }

        private void ApplyZoom(float zoomDelta, Vector2? focalPoint = null)
        {
            float oldZoom = _zoom;
            _zoom = Mathf.Clamp(_zoom + zoomDelta, 0.125f, 4.0f);
            
            if (Mathf.IsEqualApprox(oldZoom, _zoom))
                return; // No change, avoid division issues
            
            // Default focal point is the center of the viewport (accounting for UI panels)
            if (focalPoint == null)
            {
                var viewportSize = GetViewportRect().Size;
                const float TopPanelHeight = 40f;
                const float BottomPanelHeight = 60f;
                float availableHeight = viewportSize.Y - TopPanelHeight - BottomPanelHeight;
                focalPoint = new Vector2(
                    viewportSize.X / 2,
                    TopPanelHeight + (availableHeight / 2)
                );
            }
            
            // Adjust pan offset to keep the focal point stationary
            // Formula: newOffset = focalPoint - (focalPoint - oldOffset) * (newZoom / oldZoom)
            float zoomRatio = _zoom / oldZoom;
            _panOffset = focalPoint.Value - (focalPoint.Value - _panOffset) * zoomRatio;
            
            _mazeGrid.Scale = new Vector2(_zoom, _zoom);
            _mazeGrid.Position = _panOffset;
        }

        private void FitMazeToView(int mazeCellsX, int mazeCellsY)
        {
            // Calculate maze dimensions in pixels (unscaled)
            float mazeWidth = mazeCellsX * CellSize;
            float mazeHeight = mazeCellsY * CellSize;

            // Get viewport size and account for UI panels
            var viewportSize = GetViewportRect().Size;
            const float TopPanelHeight = 40f;
            const float BottomPanelHeight = 60f;
            const float Padding = 20f; // Extra padding around the maze

            float availableWidth = viewportSize.X - (Padding * 2);
            float availableHeight = viewportSize.Y - TopPanelHeight - BottomPanelHeight - (Padding * 2);

            // Calculate zoom to fit maze in available space
            // Use the smaller scale factor to ensure both dimensions fit
            float scaleX = availableWidth / mazeWidth;
            float scaleY = availableHeight / mazeHeight;
            float fitZoom = Mathf.Min(scaleX, scaleY);

            // Clamp zoom to valid range, but allow going below 1.0 for large mazes
            _zoom = Mathf.Clamp(fitZoom, 0.125f, 4.0f);
            _mazeGrid.Scale = new Vector2(_zoom, _zoom);

            // Calculate the scaled maze size
            float scaledMazeWidth = mazeWidth * _zoom;
            float scaledMazeHeight = mazeHeight * _zoom;

            // Center the maze in the available area (between top and bottom panels)
            float availableCenterX = viewportSize.X / 2;
            float availableCenterY = TopPanelHeight + (availableHeight / 2) + Padding;

            // Position so the maze center aligns with the available area center
            _panOffset = new Vector2(
                availableCenterX - (scaledMazeWidth / 2),
                availableCenterY - (scaledMazeHeight / 2)
            );
            _mazeGrid.Position = _panOffset;
        }

        public override void _Input(InputEvent @event)
        {
            // Mouse wheel zoom (zoom toward mouse cursor)
            if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                {
                    ApplyZoom(ZoomSpeed, mouseButton.Position);
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                {
                    ApplyZoom(-ZoomSpeed, mouseButton.Position);
                }
            }
        }

        private void OnDeadEndsToggled(bool pressed)
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.HideDeadEnds = pressed;
                RenderMaze();
            }
        }

        private void OnShowPathToggled(bool pressed)
        {
            SetShowPath(pressed);
        }

        private void OnShowGraphToggled(bool pressed)
        {
            SetShowGraph(pressed);
        }

        private void SetShowPath(bool enabled)
        {
            if (GameState.Instance == null) return;
            
            GameState.Instance.ShowPath = enabled;
            _showPathCheck.ButtonPressed = enabled;
            
            // Mutual exclusivity: disable graph when enabling path
            if (enabled && GameState.Instance.ShowGraph)
            {
                GameState.Instance.ShowGraph = false;
                _showGraphCheck.ButtonPressed = false;
            }
            RenderMaze();
        }

        private void SetShowGraph(bool enabled)
        {
            if (GameState.Instance == null) return;
            
            GameState.Instance.ShowGraph = enabled;
            _showGraphCheck.ButtonPressed = enabled;
            
            // Mutual exclusivity: disable path when enabling graph
            if (enabled && GameState.Instance.ShowPath)
            {
                GameState.Instance.ShowPath = false;
                _showPathCheck.ButtonPressed = false;
            }
            RenderMaze();
        }

        private void OnShowGraphViewToggled(bool pressed)
        {
            SetShowGraphView(pressed);
        }

        private void SetShowGraphView(bool enabled)
        {
            if (GameState.Instance == null) return;
            
            GameState.Instance.ShowGraphView = enabled;
            _showGraphViewCheck.ButtonPressed = enabled;
            
            // When enabling graph view, it's a completely different view mode
            // Disable other overlays as they don't apply
            if (enabled)
            {
                GameState.Instance.ShowPath = false;
                GameState.Instance.ShowGraph = false;
                _showPathCheck.ButtonPressed = false;
                _showGraphCheck.ButtonPressed = false;
                
                // Compute alternative paths if not already done
                EnsureAlternativePathsComputed();
            }
            RenderMaze(centerView: true); // Re-center for the new view
            UpdateUI();
        }

        #region Alternative Paths

        private void EnsureAlternativePathsComputed()
        {
            var gameState = GameState.Instance;
            if (gameState == null || gameState.CurrentMaze == null) return;
            if (gameState.AlternativePathsComputed) return;

            var graph = gameState.CurrentMaze.HeuristicsResults?.ShortestPathResult?.Graph;
            var mazeJumper = gameState.CurrentMaze.MazeJumper;
            
            if (graph == null) return;

            // Use the K-shortest paths solver
            var kSolver = gameState.Services.KShortestPathsSolver;
            int maxPaths = gameState.VisualizationSettings.MaxAlternativePaths;
            
            // Cap K for large mazes
            var size = mazeJumper.Size;
            if (size.X * size.Y >= 2500) // 50x50 or larger
            {
                maxPaths = Math.Min(maxPaths, 5);
            }

            var paths = kSolver.GetKShortestPaths(graph, mazeJumper.StartPoint, mazeJumper.EndPoint, maxPaths);
            
            gameState.AllPaths = paths;
            gameState.CurrentPathIndex = 0;
            gameState.AlternativePathsComputed = true;
        }

        private void CycleToNextPath()
        {
            var gameState = GameState.Instance;
            if (gameState == null) return;
            
            EnsureAlternativePathsComputed();
            
            if (gameState.AllPaths.Count <= 1)
            {
                // Only one path exists (perfect maze)
                return;
            }
            
            gameState.NextPath();
            RenderMaze();
            UpdatePathIndexLabel();
        }

        private void CycleToPreviousPath()
        {
            var gameState = GameState.Instance;
            if (gameState == null) return;
            
            EnsureAlternativePathsComputed();
            
            if (gameState.AllPaths.Count <= 1)
            {
                return;
            }
            
            gameState.PreviousPath();
            RenderMaze();
            UpdatePathIndexLabel();
        }

        private void ToggleAllPathsView()
        {
            var gameState = GameState.Instance;
            if (gameState == null) return;
            
            EnsureAlternativePathsComputed();
            
            gameState.VisualizationSettings.ShowAllPathsSimultaneously = 
                !gameState.VisualizationSettings.ShowAllPathsSimultaneously;
            
            RenderMaze();
            UpdatePathIndexLabel();
        }

        private void UpdatePathIndexLabel()
        {
            var gameState = GameState.Instance;
            if (gameState == null || _pathIndexLabel == null) return;
            
            if (!gameState.ShowGraphView)
            {
                _pathIndexLabel.Visible = false;
                return;
            }
            
            _pathIndexLabel.Visible = true;
            UpdatePathIndexLabelPosition();
            // Reset to white color (non-animation mode)
            _pathIndexLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            
            if (gameState.AllPaths.Count <= 1)
            {
                _pathIndexLabel.Text = "1 path only";
            }
            else if (gameState.VisualizationSettings.ShowAllPathsSimultaneously)
            {
                _pathIndexLabel.Text = $"All {gameState.AllPaths.Count} paths";
            }
            else
            {
                var currentPath = gameState.CurrentPath;
                string distInfo = currentPath != null && currentPath.DistanceFromOptimal > 0 
                    ? $" (+{currentPath.DistanceFromOptimal})" 
                    : "";
                _pathIndexLabel.Text = $"Path {gameState.CurrentPathIndex + 1}/{gameState.AllPaths.Count}{distInfo}";
            }
        }

        private Label CreatePathIndexLabel()
        {
            var label = new Label();
            label.Name = "PathIndexLabel";
            label.Text = "";
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            label.AddThemeFontSizeOverride("font_size", 16);
            
            // Add to UIOverlay for proper layering
            // Position will be set dynamically by UpdatePathIndexLabelPosition()
            var uiOverlay = GetNodeOrNull<Control>("UIOverlay");
            if (uiOverlay != null)
            {
                uiOverlay.AddChild(label);
            }
            else if (_graphStatsPanel != null)
            {
                var parent = _graphStatsPanel.GetParent();
                parent?.AddChild(label);
            }
            
            label.Visible = false;
            return label;
        }

        private void UpdatePathIndexLabelPosition()
        {
            if (_pathIndexLabel == null) return;
            
            // Position dynamically below the legend panel with 10px gap
            if (_legendPanel != null && _legendPanel.Visible)
            {
                float yPos = _legendPanel.Position.Y + _legendPanel.Size.Y + 10;
                _pathIndexLabel.Position = new Vector2(10, yPos);
            }
            else
            {
                // Fallback when legend not visible
                _pathIndexLabel.Position = new Vector2(10, 60);
            }
        }

        #endregion

        #region Animation Controls

        private void ToggleAnimationMode()
        {
            var gameState = GameState.Instance;
            if (gameState == null) return;
            
            gameState.IsAnimationMode = !gameState.IsAnimationMode;
            
            if (gameState.IsAnimationMode)
            {
                // Initialize animation if not already done
                if (gameState.AnimationSteps == null || gameState.AnimationSteps.Count == 0)
                {
                    var graph = gameState.CurrentMaze?.HeuristicsResults?.ShortestPathResult?.Graph;
                    var mazeJumper = gameState.CurrentMaze?.MazeJumper;
                    
                    if (graph != null && mazeJumper != null)
                    {
                        var animator = gameState.Services.DijkstraAnimator;
                        gameState.AnimationSteps = animator.CaptureSteps(graph, mazeJumper.StartPoint, mazeJumper.EndPoint);
                        gameState.AnimationController = new AnimationController(
                            gameState.AnimationSteps, 
                            gameState.VisualizationSettings.DefaultPlaybackSpeed
                        );
                    }
                }
            }
            
            RenderMaze();
            UpdateUI();
        }

        private void UpdateAnimationUI()
        {
            var gameState = GameState.Instance;
            if (gameState == null) return;
            
            // Update the path index label to show animation status
            if (_pathIndexLabel != null && gameState.IsAnimationMode && gameState.AnimationController != null)
            {
                var controller = gameState.AnimationController;
                var step = controller.CurrentStep;
                
                string stateStr = controller.State switch
                {
                    PlaybackState.Playing => "Playing",
                    PlaybackState.Paused => "Paused",
                    PlaybackState.Stopped => "Stopped",
                    PlaybackState.Finished => "Complete",
                    _ => ""
                };
                
            _pathIndexLabel.Visible = true;
            UpdatePathIndexLabelPosition();
                // Set orange color for animation mode
                _pathIndexLabel.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.2f));
                _pathIndexLabel.Text = $"{stateStr} {controller.GetProgressString()} ({controller.GetSpeedString()})";
            }
        }

        #endregion

        #region Decision Points

        private void CycleDecisionPointLevel()
        {
            var gameState = GameState.Instance;
            if (gameState == null) return;
            
            // Cycle: Off -> Badges -> Detailed -> Off
            gameState.VisualizationSettings.DecisionDetailLevel = gameState.VisualizationSettings.DecisionDetailLevel switch
            {
                DecisionDetailLevel.Off => DecisionDetailLevel.Badges,
                DecisionDetailLevel.Badges => DecisionDetailLevel.Detailed,
                DecisionDetailLevel.Detailed => DecisionDetailLevel.Off,
                _ => DecisionDetailLevel.Off
            };
            
            RenderMaze();
            UpdateUI();
        }

        #endregion

        private void RenderMaze(bool centerView = false)
        {
            // Clear existing cells
            foreach (Node child in _mazeGrid.GetChildren())
            {
                child.QueueFree();
            }

            var gameState = GameState.Instance;
            if (gameState?.CurrentMaze == null) return;

            // Check if we're in graph view mode
            if (gameState.ShowGraphView)
            {
                _graphViewRenderer.Render(gameState, centerView);
                return;
            }

            var mazeJumper = gameState.CurrentMaze.MazeJumper;
            var currentZ = gameState.CurrentLevel;
            var hideDeadEnds = gameState.HideDeadEnds;

            // Set model mode based on dead ends setting
            // HideDeadEnds=true means hide dead ends (DeadEndFilled mode hides dead-end passages)
            // HideDeadEnds=false means show dead ends (Standard mode shows all passages)
            mazeJumper.SetState(hideDeadEnds ? ModelMode.DeadEndFilled : ModelMode.Standard);

            // Fit maze to view on initial load
            if (centerView)
            {
                FitMazeToView(mazeJumper.Size.X, mazeJumper.Size.Y);
            }

            // Render each cell on the current level
            // Flip Y axis: maze Y=0 should be at the bottom of the screen
            int maxY = mazeJumper.Size.Y - 1;
            for (int x = 0; x < mazeJumper.Size.X; x++)
            {
                for (int y = 0; y < mazeJumper.Size.Y; y++)
                {
                    var point = new MazePoint(x, y, currentZ);
                    mazeJumper.JumpToPoint(point);
                    var directions = mazeJumper.GetFlagFromPoint();

                    // Flip Y for screen rendering: screenY = maxY - mazeY
                    int screenY = maxY - y;
                    var cellSprite = CreateCellSprite(x, screenY, directions, point, mazeJumper);
                    if (cellSprite != null)
                    {
                        _mazeGrid.AddChild(cellSprite);
                    }
                }
            }

            // Render the solution path if enabled
            if (gameState.ShowPath)
            {
                RenderSolutionPath(gameState, currentZ);
            }

            // Render the graph representation if enabled
            if (gameState.ShowGraph)
            {
                RenderGraph(gameState, currentZ);
            }
        }

        private void RenderSolutionPath(GameState gameState, int currentZ)
        {
            if (gameState.CurrentMaze?.HeuristicsResults?.ShortestPathResult?.ShortestPathDirections == null)
                return;

            var pathDirections = gameState.CurrentMaze.HeuristicsResults.ShortestPathResult.ShortestPathDirections;
            if (pathDirections.Length == 0) return;

            var mazeJumper = gameState.CurrentMaze.MazeJumper;
            var currentPoint = new MazePoint(mazeJumper.StartPoint.X, mazeJumper.StartPoint.Y, mazeJumper.StartPoint.Z);
            int maxY = mazeJumper.Size.Y - 1;

            for (int i = 0; i < pathDirections.Length; i++)
            {
                var direction = pathDirections[i];
                bool isStartCell = (i == 0);
                
                // Render exit line from current cell (except we handle entrance/exit per cell)
                if (currentPoint.Z == currentZ)
                {
                    int screenY = maxY - currentPoint.Y;
                    
                    // Draw entrance line (from edge to center) - skip for start cell
                    if (!isStartCell)
                    {
                        var entranceDir = GetOppositeDirection(pathDirections[i - 1]);
                        var entranceSprite = CreateHalfLineSprite(currentPoint.X, screenY, entranceDir, "halfLineBlue");
                        if (entranceSprite != null)
                        {
                            _mazeGrid.AddChild(entranceSprite);
                        }
                    }
                    
                    // Draw exit line (from center to edge)
                    var exitSprite = CreateHalfLineSprite(currentPoint.X, screenY, direction, "halfLineBlue");
                    if (exitSprite != null)
                    {
                        _mazeGrid.AddChild(exitSprite);
                    }
                }

                // Move to the next cell
                currentPoint = MovePoint(currentPoint, direction);
            }

            // Render the final cell (end point) - only entrance, no exit
            if (currentPoint.Z == currentZ && pathDirections.Length > 0)
            {
                int screenY = maxY - currentPoint.Y;
                var lastDirection = pathDirections[pathDirections.Length - 1];
                var entranceDir = GetOppositeDirection(lastDirection);
                var entranceSprite = CreateHalfLineSprite(currentPoint.X, screenY, entranceDir, "halfLineBlue");
                if (entranceSprite != null)
                {
                    _mazeGrid.AddChild(entranceSprite);
                }
            }
        }

        private void RenderGraph(GameState gameState, int currentZ)
        {
            var graph = gameState.CurrentMaze?.HeuristicsResults?.ShortestPathResult?.Graph;
            if (graph == null) return;

            var mazeJumper = gameState.CurrentMaze!.MazeJumper;
            int maxY = mazeJumper.Size.Y - 1;

            foreach (var kvp in graph.Nodes)
            {
                var point = kvp.Key;
                var node = kvp.Value;

                // Render edges from this node (draw lines first, circles on top)
                foreach (var edge in node.Edges)
                {
                    RenderGraphEdge(point, edge, currentZ, maxY);
                }
            }

            // Render circles on top of lines
            foreach (var kvp in graph.Nodes)
            {
                var point = kvp.Key;

                // Only render nodes on current Z level
                if (point.Z != currentZ) continue;

                int screenY = maxY - point.Y;

                // Choose circle color based on whether it's start, end, or regular junction
                string circleName;
                if (point.Equals(mazeJumper.StartPoint))
                {
                    circleName = "greenStartCircle";
                }
                else if (point.Equals(mazeJumper.EndPoint))
                {
                    circleName = "redEndCircle";
                }
                else
                {
                    circleName = "orangeCircle";
                }

                var circleSprite = CreateMarkerSprite(circleName, point.X, screenY);
                if (circleSprite != null)
                {
                    circleSprite.ZIndex = 3; // Above lines
                    _mazeGrid.AddChild(circleSprite);
                }
            }
        }

        private void RenderGraphEdge(MazePoint startPoint, GraphEdge edge, int currentZ, int maxY)
        {
            // Walk through each step of the edge and draw orange lines
            var currentPoint = startPoint;

            for (int i = 0; i < edge.DirectionsToPoint.Length; i++)
            {
                var direction = edge.DirectionsToPoint[i];

                // Only render if current point is on this Z level
                if (currentPoint.Z == currentZ)
                {
                    int screenY = maxY - currentPoint.Y;

                    // Draw exit half-line (orange)
                    var exitSprite = CreateHalfLineSprite(currentPoint.X, screenY, direction, "halfLineOrange");
                    if (exitSprite != null)
                    {
                        _mazeGrid.AddChild(exitSprite);
                    }
                }

                // Move to next point
                var nextPoint = MovePoint(currentPoint, direction);

                // Draw entrance half-line at next point
                if (nextPoint.Z == currentZ)
                {
                    int screenY = maxY - nextPoint.Y;
                    var entranceDir = GetOppositeDirection(direction);
                    var entranceSprite = CreateHalfLineSprite(nextPoint.X, screenY, entranceDir, "halfLineOrange");
                    if (entranceSprite != null)
                    {
                        _mazeGrid.AddChild(entranceSprite);
                    }
                }

                currentPoint = nextPoint;
            }
        }

        private void FitGraphViewToScreen(float graphWidth, float graphHeight, float offsetX, float offsetY)
        {
            var viewportSize = GetViewportRect().Size;
            const float TopPanelHeight = 40f;
            const float BottomPanelHeight = 60f;
            const float Padding = 40f;

            float availableWidth = viewportSize.X - (Padding * 2);
            float availableHeight = viewportSize.Y - TopPanelHeight - BottomPanelHeight - (Padding * 2);

            float scaleX = availableWidth / graphWidth;
            float scaleY = availableHeight / graphHeight;
            float fitZoom = Math.Min(scaleX, scaleY);

            _zoom = Mathf.Clamp(fitZoom, 0.125f, 2.0f);
            _mazeGrid.Scale = new Vector2(_zoom, _zoom);

            float scaledWidth = graphWidth * _zoom;
            float scaledHeight = graphHeight * _zoom;

            float availableCenterX = viewportSize.X / 2;
            float availableCenterY = TopPanelHeight + (availableHeight / 2) + Padding;

            _panOffset = new Vector2(
                availableCenterX - (scaledWidth / 2) - (offsetX * _zoom),
                availableCenterY - (scaledHeight / 2) - (offsetY * _zoom)
            );
            _mazeGrid.Position = _panOffset;
        }

        private bool IsVerticalDirection(Direction direction)
        {
            return direction == Direction.Up || direction == Direction.Down;
        }

        private Sprite2D? CreateHalfLineSprite(int x, int y, Direction direction, string spriteName)
        {
            if (!_sprites.ContainsKey(spriteName)) return null;

            bool isVertical = IsVerticalDirection(direction);

            var sprite = new Sprite2D();
            sprite.Texture = _sprites[spriteName];
            
            // Half line is 75px, we want it to span half the cell (16px)
            float lineScale = (CellSize / 2f) / 75f;
            float widthScale = lineScale * 1.15f;  // Slightly thicker lines
            float offsetAmount = CellSize / 4f;
            
            if (isVertical)
            {
                // Slightly adjust diagonal lines
                lineScale *= 0.72f;
                widthScale *= 1.1f;
                offsetAmount *= 0.65f;
            }
            
            // Position: offset from center toward the direction
            Vector2 offset = GetDirectionOffset(direction) * offsetAmount;
            sprite.Position = new Vector2(x * CellSize + CellSize / 2f + offset.X, y * CellSize + CellSize / 2f + offset.Y);
            
            sprite.Scale = new Vector2(lineScale, widthScale);
            sprite.ZIndex = 2; // Render above cells and markers
            sprite.RotationDegrees = GetRotationForDirection(direction);

            return sprite;
        }

        private Vector2 GetDirectionOffset(Direction direction)
        {
            // Returns unit vector for the direction (in screen coordinates with Y-flip applied)
            // Up stairs are top-right, Down stairs are bottom-left
            return direction switch
            {
                Direction.Right => new Vector2(1, 0),
                Direction.Left => new Vector2(-1, 0),
                Direction.Forward => new Vector2(0, -1),  // Up on screen after Y-flip
                Direction.Back => new Vector2(0, 1),      // Down on screen after Y-flip
                Direction.Up => new Vector2(1, -1).Normalized(),    // Diagonal top-right
                Direction.Down => new Vector2(-1, 1).Normalized(),  // Diagonal bottom-left
                _ => Vector2.Zero
            };
        }

        private float GetRotationForDirection(Direction direction)
        {
            // With Y-flipped rendering:
            // Forward (Y+1 in maze) now points UP on screen (negative screen Y)
            // Back (Y-1 in maze) now points DOWN on screen (positive screen Y)
            // Up stairs are top-right, Down stairs are bottom-left
            return direction switch
            {
                Direction.Right => 0f,
                Direction.Forward => -90f,  // Up on screen (after Y flip)
                Direction.Left => 180f,
                Direction.Back => 90f,      // Down on screen (after Y flip)
                Direction.Up => -45f,       // Diagonal toward top-right (where up stairs are)
                Direction.Down => 135f,     // Diagonal toward bottom-left (where down stairs are)
                _ => 0f
            };
        }

        private Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                Direction.Forward => Direction.Back,
                Direction.Back => Direction.Forward,
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                _ => Direction.None
            };
        }

        private MazePoint MovePoint(MazePoint point, Direction direction)
        {
            // Match MovementHelper.cs coordinate system:
            // Forward = Y+1, Back = Y-1
            return direction switch
            {
                Direction.Left => new MazePoint(point.X - 1, point.Y, point.Z),
                Direction.Right => new MazePoint(point.X + 1, point.Y, point.Z),
                Direction.Forward => new MazePoint(point.X, point.Y + 1, point.Z),
                Direction.Back => new MazePoint(point.X, point.Y - 1, point.Z),
                Direction.Up => new MazePoint(point.X, point.Y, point.Z + 1),
                Direction.Down => new MazePoint(point.X, point.Y, point.Z - 1),
                _ => point
            };
        }

        private Sprite2D? CreateCellSprite(int x, int y, Direction directions, MazePoint point, IMazeJumper mazeJumper)
        {
            var spriteName = GetSpriteNameForDirections(directions);
            if (string.IsNullOrEmpty(spriteName) || !_sprites.ContainsKey(spriteName))
            {
                // Create a simple white cell for debugging
                spriteName = "White";
                if (!_sprites.ContainsKey(spriteName)) return null;
            }

            var sprite = new Sprite2D();
            sprite.Texture = _sprites[spriteName];
            sprite.Position = new Vector2(x * CellSize + CellSize / 2f, y * CellSize + CellSize / 2f);
            sprite.Scale = new Vector2(_cellScale, _cellScale);

            // Add stair overlays for vertical movement
            if (HasUpStairs(directions))
            {
                var upStairs = CreateStairSprite("UpStairs", x, y, isUp: true);
                if (upStairs != null)
                {
                    _mazeGrid.AddChild(upStairs);
                }
            }
            if (HasDownStairs(directions))
            {
                var downStairs = CreateStairSprite("DownStairs", x, y, isUp: false);
                if (downStairs != null)
                {
                    _mazeGrid.AddChild(downStairs);
                }
            }

            // Check if this is start or end point
            if (point.Equals(mazeJumper.StartPoint))
            {
                var startMarker = CreateMarkerSprite("greenStartCircle", x, y);
                if (startMarker != null)
                {
                    _mazeGrid.AddChild(startMarker);
                }
            }
            else if (point.Equals(mazeJumper.EndPoint))
            {
                var endMarker = CreateMarkerSprite("redEndCircle", x, y);
                if (endMarker != null)
                {
                    _mazeGrid.AddChild(endMarker);
                }
            }

            return sprite;
        }

        private Sprite2D? CreateStairSprite(string spriteName, int x, int y, bool isUp)
        {
            if (!_sprites.ContainsKey(spriteName)) return null;

            var sprite = new Sprite2D();
            sprite.Texture = _sprites[spriteName];
            
            // Position in corner: top-right for up stairs, bottom-left for down stairs
            float offset = CellSize * 0.3f;
            float centerX = x * CellSize + CellSize / 2f;
            float centerY = y * CellSize + CellSize / 2f;
            
            if (isUp)
            {
                // Top-right corner
                sprite.Position = new Vector2(centerX + offset, centerY - offset);
            }
            else
            {
                // Bottom-left corner
                sprite.Position = new Vector2(centerX - offset, centerY + offset);
            }
            
            sprite.Scale = new Vector2(_stairScale, _stairScale);
            sprite.ZIndex = 1; // Render above base cell
            return sprite;
        }

        private Sprite2D? CreateMarkerSprite(string spriteName, int x, int y)
        {
            if (!_sprites.ContainsKey(spriteName)) return null;

            var sprite = new Sprite2D();
            sprite.Texture = _sprites[spriteName];
            sprite.Position = new Vector2(x * CellSize + CellSize / 2f, y * CellSize + CellSize / 2f);
            sprite.Scale = new Vector2(_markerScale, _markerScale);
            sprite.ZIndex = 1; // Render above cells
            return sprite;
        }

        private string GetSpriteNameForDirections(Direction directions)
        {
            // Handle no directions case
            if (directions == Direction.None)
            {
                return "Cover";
            }

            // Build sprite name from horizontal directions only (Left, Right, Forward, Back)
            // Order must be: Left, Right, Forward, Back (to match sprite file names)
            var spriteName = "";
            
            if ((directions & Direction.Left) != 0)
            {
                spriteName += "Left";
            }
            if ((directions & Direction.Right) != 0)
            {
                spriteName += "Right";
            }
            if ((directions & Direction.Forward) != 0)
            {
                spriteName += "Forward";
            }
            if ((directions & Direction.Back) != 0)
            {
                spriteName += "Back";
            }

            // If only vertical directions (Up/Down), use Uncover as base
            if (string.IsNullOrEmpty(spriteName))
            {
                return "Uncover";
            }

            return spriteName;
        }

        private bool HasUpStairs(Direction directions)
        {
            return (directions & Direction.Up) != 0;
        }

        private bool HasDownStairs(Direction directions)
        {
            return (directions & Direction.Down) != 0;
        }

        private void UpdateUI()
        {
            var gameState = GameState.Instance;
            if (gameState?.CurrentMaze == null) return;

            var maze = gameState.CurrentMaze;
            var size = maze.MazeJumper.Size;

            _mazeInfoLabel.Text = $"Maze: {size.X}x{size.Y}x{size.Z}";
            _levelLabel.Text = $"Level: {gameState.CurrentLevel + 1}/{size.Z}";
            _statsLabel.Text = $"Path: {maze.HeuristicsResults.ShortestPathResult.ShortestPath} | Time: {maze.TotalTime.TotalMilliseconds:F0}ms";
            
            UpdateControlsForView();
        }

        private void UpdateControlsForView()
        {
            var gameState = GameState.Instance;
            if (gameState == null) return;

            // Show/hide graph view specific panels
            bool isGraphView = gameState.ShowGraphView;
            _legendPanel.Visible = isGraphView;
            _graphStatsPanel.Visible = isGraphView;

            if (isGraphView)
            {
                // Check if in animation mode
                if (gameState.IsAnimationMode)
                {
                    UpdateAnimationUI();
                }
                else
                {
                    UpdatePathIndexLabel();
                }
                
                // Update graph statistics
                UpdateGraphStats(gameState);
            }
        }

        private void UpdateGraphStats(GameState gameState)
        {
            var graph = gameState.CurrentMaze?.HeuristicsResults?.ShortestPathResult?.Graph;
            if (graph == null) return;

            int nodeCount = 0;
            int edgeCount = 0;
            int junctionCount = 0;

            foreach (var kvp in graph.Nodes)
            {
                var node = kvp.Value;
                if (node.ShortestPath == int.MaxValue) continue;
                
                nodeCount++;
                edgeCount += node.Edges.Length;
                
                // A junction has more than 2 edges (not just a corridor)
                if (node.Edges.Length > 2)
                {
                    junctionCount++;
                }
            }

            // Each edge is counted twice (once from each endpoint)
            edgeCount /= 2;

            var pathLength = gameState.CurrentMaze?.HeuristicsResults?.ShortestPathResult?.ShortestPath ?? 0;

            _nodesLabel.Text = $"Nodes: {nodeCount}";
            _edgesLabel.Text = $"Edges: {edgeCount}";
            _pathLengthLabel.Text = $"Path length: {pathLength}";
            _junctionsLabel.Text = $"Junctions: {junctionCount}";
        }

        #region Import/Export

        private void QuickExport()
        {
            var gameState = GameState.Instance;
            if (gameState?.CurrentMaze == null)
            {
                _toast.ShowError("No maze to export");
                return;
            }

            var model = gameState.CurrentMaze.MazeJumper.GetModel();
            var result = MazeImportExport.QuickExport(model, gameState.Services.MazeSerializer);

            if (result.Success)
            {
                _toast.ShowSuccess($"Maze exported to {System.IO.Path.GetFileName(result.FilePath)}",
                    result.FilePath);
            }
            else
            {
                _toast.ShowError(result.ErrorMessage ?? "Export failed", result.TechnicalDetails);
            }
        }

        private void ShowExportDialog()
        {
            var gameState = GameState.Instance;
            if (gameState?.CurrentMaze == null)
            {
                _toast.ShowError("No maze to export");
                return;
            }

            // Set suggested filename
            var size = gameState.CurrentMaze.MazeJumper.Size;
            _exportDialog.CurrentFile = MazeImportExport.GenerateDefaultFilename(size);
            _exportDialog.PopupCentered();
        }

        private void ShowImportDialog()
        {
            _importDialog.PopupCentered();
        }

        private void OnExportFileSelected(string path)
        {
            var gameState = GameState.Instance;
            if (gameState?.CurrentMaze == null)
            {
                _toast.ShowError("No maze to export");
                return;
            }

            var model = gameState.CurrentMaze.MazeJumper.GetModel();
            var result = MazeImportExport.ExportTo(model, path, gameState.Services.MazeSerializer);

            if (result.Success)
            {
                // Also export the stats file alongside the maze
                var statsResult = MazeImportExport.ExportStats(
                    gameState.CurrentMaze,
                    path,
                    gameState.Services.MazeStatsSerializer);

                if (statsResult.Success)
                {
                    _toast.ShowSuccess(
                        $"Maze and stats exported to {System.IO.Path.GetFileName(result.FilePath)}",
                        result.FilePath);
                }
                else
                {
                    // Maze exported but stats failed - still show success with warning
                    _toast.ShowSuccess(
                        $"Maze exported to {System.IO.Path.GetFileName(result.FilePath)} (stats export failed)",
                        result.FilePath);
                }
            }
            else
            {
                _toast.ShowError(result.ErrorMessage ?? "Export failed", result.TechnicalDetails);
            }
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
                gameState.LoadImportedMaze(result.Model);
                RenderMaze(centerView: true);
                UpdateUI();
                _toast.ShowSuccess($"Maze imported: {result.Model.Size.X}x{result.Model.Size.Y}x{result.Model.Size.Z}",
                    System.IO.Path.GetFileName(path));
            }
            else
            {
                _toast.ShowError(result.ErrorMessage ?? "Import failed", result.TechnicalDetails);
            }
        }

        #endregion
    }
}

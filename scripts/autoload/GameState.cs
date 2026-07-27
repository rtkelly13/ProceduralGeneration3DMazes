using System.Collections.Generic;
using Godot;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.Session;
using ProceduralMaze.UI;

namespace ProceduralMaze.Autoload
{
    /// <summary>
    /// Autoload singleton exposing global game state. Register in project.godot under
    /// [autoload] as "GameState".
    /// </summary>
    /// <remarks>
    /// Thin adapter over <see cref="MazeSession"/>, which holds the actual state and logic and
    /// has no Godot dependency. This class exists only to be a <c>Node</c> — everything
    /// meaningful forwards to the session, so the flows the app runs are testable in-process
    /// without the engine (see tests/MazeSessionTests.cs).
    ///
    /// Before the split this class was 266 lines whose entire Godot surface was <c>: Node</c>,
    /// <c>_Ready</c>, <c>_ExitTree</c>, one <c>GD.Print</c> and one <c>Mathf.Clamp</c> — nearly
    /// all logic, none of it testable, because the NUnit suite builds without the Godot SDK.
    ///
    /// The public API is unchanged, so callers in scripts/ui and scripts/testing need no edits.
    ///
    /// One thing stays here deliberately: <see cref="VisualizationSettings"/> is presentation
    /// config built on Godot's <c>Color</c>, so moving it into the session would reintroduce
    /// the very dependency the split removes.
    /// </remarks>
    public partial class GameState : Node
    {
        public static GameState? Instance { get; private set; }

        /// <summary>The Godot-free session this node wraps.</summary>
        public MazeSession Session { get; private set; } = null!;

        /// <summary>Service container for dependency injection.</summary>
        public ServiceContainer Services => Session.Services;

        public MazeGenerationSettings Settings
        {
            get => Session.Settings;
            set => Session.Settings = value;
        }

        public MazeGenerationResults? CurrentMaze
        {
            get => Session.CurrentMaze;
            set => Session.CurrentMaze = value;
        }

        public int CurrentLevel
        {
            get => Session.CurrentLevel;
            set => Session.CurrentLevel = value;
        }

        public bool HideDeadEnds
        {
            get => Session.HideDeadEnds;
            set => Session.HideDeadEnds = value;
        }

        public bool ShowPath
        {
            get => Session.ShowPath;
            set => Session.ShowPath = value;
        }

        public bool ShowGraph
        {
            get => Session.ShowGraph;
            set => Session.ShowGraph = value;
        }

        public bool ShowGraphView
        {
            get => Session.ShowGraphView;
            set => Session.ShowGraphView = value;
        }

        public GraphLayoutType GraphLayout
        {
            get => Session.GraphLayout;
            set => Session.GraphLayout = value;
        }

        #region Alternative Paths

        public List<PathResult> AllPaths
        {
            get => Session.AllPaths;
            set => Session.AllPaths = value;
        }

        public int CurrentPathIndex
        {
            get => Session.CurrentPathIndex;
            set => Session.CurrentPathIndex = value;
        }

        public PathResult? CurrentPath => Session.CurrentPath;

        public bool AlternativePathsComputed
        {
            get => Session.AlternativePathsComputed;
            set => Session.AlternativePathsComputed = value;
        }

        #endregion

        #region Animation State

        public bool IsAnimationMode
        {
            get => Session.IsAnimationMode;
            set => Session.IsAnimationMode = value;
        }

        public List<AlgorithmStep>? AnimationSteps
        {
            get => Session.AnimationSteps;
            set => Session.AnimationSteps = value;
        }

        public AnimationController? AnimationController
        {
            get => Session.AnimationController;
            set => Session.AnimationController = value;
        }

        #endregion

        #region Visualization Settings

        /// <summary>
        /// Path visualization configuration. Stays on the node rather than in the session
        /// because it is built on Godot's <c>Color</c>.
        /// </summary>
        public PathVisualizationSettings VisualizationSettings { get; set; } = new();

        #endregion

        #region Path Navigation Methods

        public void NextPath() => Session.NextPath();

        public void PreviousPath() => Session.PreviousPath();

        /// <summary>
        /// Resets visualization state when generating a new maze: the session's path and
        /// animation state, plus the presentation flags this node owns.
        /// </summary>
        public void ResetVisualizationState()
        {
            Session.ResetVisualizationState();
            ResetPresentationFlags();
        }

        private void ResetPresentationFlags()
        {
            VisualizationSettings.ShowAllPathsSimultaneously = false;
            VisualizationSettings.AnimationEnabled = false;
            VisualizationSettings.DecisionDetailLevel = DecisionDetailLevel.Off;
        }

        #endregion

        public override void _Ready()
        {
            Instance = this;
            Session = new MazeSession();
            GD.Print("GameState initialized with ServiceContainer");
        }

        public override void _ExitTree()
        {
            Instance = null;
        }

        /// <summary>Generates a new maze with the current settings.</summary>
        public MazeGenerationResults GenerateMaze() => Session.GenerateMaze();

        /// <summary>
        /// Loads an imported maze from a model, computing heuristics and dead-end wrapping.
        /// </summary>
        public MazeGenerationResults LoadImportedMaze(IModel model)
        {
            var results = Session.LoadImportedMaze(model);
            // The session resets its own state; the presentation flags are this node's.
            ResetPresentationFlags();
            return results;
        }

        /// <summary>Changes the current level within the valid range.</summary>
        public void SetLevel(int level) => Session.SetLevel(level);

        /// <summary>Moves to the next level if possible.</summary>
        public void NextLevel() => Session.NextLevel();

        /// <summary>Moves to the previous level if possible.</summary>
        public void PreviousLevel() => Session.PreviousLevel();
    }
}

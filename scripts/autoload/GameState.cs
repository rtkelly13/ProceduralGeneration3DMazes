using System;
using System.Collections.Generic;
using Godot;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Heuristics;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.UI;

namespace ProceduralMaze.Autoload
{
    /// <summary>
    /// Autoload singleton that holds global game state and provides access to services.
    /// Register this in project.godot under [autoload] as "GameState".
    /// </summary>
    public partial class GameState : Node
    {
        public static GameState? Instance { get; private set; }

        // Service container for dependency injection
        public ServiceContainer Services { get; private set; } = null!;

        // Current maze generation settings
        public MazeGenerationSettings Settings { get; set; } = new()
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

        // Current maze generation results (after generation is complete)
        public MazeGenerationResults? CurrentMaze { get; set; }

        // Current level being displayed (Z index for 3D mazes)
        public int CurrentLevel { get; set; } = 0;

        // Whether to hide dead ends (true = hide dead-end passages, false = show all passages)
        public bool HideDeadEnds { get; set; } = false;

        // Whether to show the solution path
        public bool ShowPath { get; set; } = false;

        // Whether to show the graph representation
        public bool ShowGraph { get; set; } = false;

        // Whether to show the abstract graph view (alternative visualization mode)
        public bool ShowGraphView { get; set; } = false;

        // Current graph layout type for graph view mode
        public GraphLayoutType GraphLayout { get; set; } = GraphLayoutType.GridAware;

        #region Alternative Paths

        /// <summary>
        /// All computed paths for the current maze (index 0 = optimal).
        /// </summary>
        public List<PathResult> AllPaths { get; set; } = new();

        /// <summary>
        /// Currently selected path index.
        /// </summary>
        public int CurrentPathIndex { get; set; } = 0;

        /// <summary>
        /// Gets the currently selected path, or null if no paths exist.
        /// </summary>
        public PathResult? CurrentPath => AllPaths.Count > 0 ? AllPaths[CurrentPathIndex] : null;

        /// <summary>
        /// Whether alternative paths have been computed.
        /// </summary>
        public bool AlternativePathsComputed { get; set; } = false;

        #endregion

        #region Animation State

        /// <summary>
        /// Whether animation mode is currently active.
        /// </summary>
        public bool IsAnimationMode { get; set; } = false;

        /// <summary>
        /// Animation steps for the current path.
        /// </summary>
        public List<AlgorithmStep>? AnimationSteps { get; set; }

        /// <summary>
        /// Animation controller for playback.
        /// </summary>
        public AnimationController? AnimationController { get; set; }

        #endregion

        #region Visualization Settings

        /// <summary>
        /// Configuration for path visualization features.
        /// </summary>
        public PathVisualizationSettings VisualizationSettings { get; set; } = new();

        #endregion

        #region Path Navigation Methods

        /// <summary>
        /// Moves to the next alternative path.
        /// </summary>
        public void NextPath()
        {
            if (AllPaths.Count > 1)
            {
                CurrentPathIndex = (CurrentPathIndex + 1) % AllPaths.Count;
            }
        }

        /// <summary>
        /// Moves to the previous alternative path.
        /// </summary>
        public void PreviousPath()
        {
            if (AllPaths.Count > 1)
            {
                CurrentPathIndex = (CurrentPathIndex - 1 + AllPaths.Count) % AllPaths.Count;
            }
        }

        /// <summary>
        /// Resets visualization state when generating a new maze.
        /// </summary>
        public void ResetVisualizationState()
        {
            AllPaths.Clear();
            CurrentPathIndex = 0;
            AlternativePathsComputed = false;
            IsAnimationMode = false;
            AnimationSteps = null;
            AnimationController = null;
            VisualizationSettings.ShowAllPathsSimultaneously = false;
            VisualizationSettings.AnimationEnabled = false;
            VisualizationSettings.DecisionDetailLevel = DecisionDetailLevel.Off;
        }

        #endregion

        public override void _Ready()
        {
            Instance = this;
            Services = new ServiceContainer();
            GD.Print("GameState initialized with ServiceContainer");
        }

        public override void _ExitTree()
        {
            Instance = null;
        }

        /// <summary>
        /// Generates a new maze with the current settings.
        /// </summary>
        public MazeGenerationResults GenerateMaze()
        {
            CurrentMaze = Services.MazeGenerationFactory.GenerateMaze(Settings);
            CurrentLevel = 0;
            return CurrentMaze;
        }

        /// <summary>
        /// Loads an imported maze from a model.
        /// Computes heuristics (shortest path, graph) and sets up dead-end wrapping.
        /// </summary>
        public MazeGenerationResults LoadImportedMaze(IModel model)
        {
            // Create a MazeJumper from the imported model
            var mazeJumper = Services.MazeFactory.GetMazeJumperFromModel(model);

            // Set up dead-end wrapping so dead-end hiding works
            mazeJumper.DoDeadEndWrapping(modelBuilder =>
                Services.DeadEndModelWrapperFactory.MakeModel(modelBuilder));

            // Compute shortest path and graph
            var shortestPathResult = Services.ShortestPathSolver.GetGraph(mazeJumper);

            // Create heuristics results (stats are placeholder for imported mazes)
            var heuristicsResults = new HeuristicsResults
            {
                TotalCells = model.Size.X * model.Size.Y * model.Size.Z,
                ShortestPathResult = shortestPathResult,
                Stats = new MazeStatsResult
                {
                    DirectionsUsed = new Dictionary<Direction, int>(),
                    MaximumUse = new DirectionResult { Direction = Direction.None, NumberOfUsages = 0 },
                    MinimumUse = new DirectionResult { Direction = Direction.None, NumberOfUsages = 0 }
                }
            };

            // Create the results object
            CurrentMaze = new MazeGenerationResults
            {
                MazeJumper = mazeJumper,
                HeuristicsResults = heuristicsResults,
                DeadEndFillerResults = new DeadEndFillerResult
                {
                    CellsFilledIn = new List<CarvedCellResult>(),
                    TotalCellsFilledIn = 0
                },
                AgentResults = null,
                ModelTime = TimeSpan.Zero,
                GenerationTime = TimeSpan.Zero,
                DeadEndFillerTime = TimeSpan.Zero,
                AgentGenerationTime = TimeSpan.Zero,
                HeuristicsTime = TimeSpan.Zero,
                TotalTime = TimeSpan.Zero,
                DirectionsCarvedIn = new List<DirectionAndPoint>()
            };

            // Update settings to reflect imported maze size
            Settings.Size = model.Size;

            CurrentLevel = 0;
            ResetVisualizationState();

            return CurrentMaze;
        }

        /// <summary>
        /// Changes the current level within the valid range.
        /// </summary>
        public void SetLevel(int level)
        {
            if (CurrentMaze != null)
            {
                var maxLevel = Settings.Size.Z - 1;
                CurrentLevel = Mathf.Clamp(level, 0, maxLevel);
            }
        }

        /// <summary>
        /// Moves to the next level if possible.
        /// </summary>
        public void NextLevel()
        {
            SetLevel(CurrentLevel + 1);
        }

        /// <summary>
        /// Moves to the previous level if possible.
        /// </summary>
        public void PreviousLevel()
        {
            SetLevel(CurrentLevel - 1);
        }
    }
}

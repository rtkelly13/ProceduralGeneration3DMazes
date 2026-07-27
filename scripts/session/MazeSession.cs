using System;
using System.Collections.Generic;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Generation;
using ProceduralMaze.Maze.Heuristics;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.UI;

namespace ProceduralMaze.Session
{
    /// <summary>
    /// All application session state and the operations on it — generation, imported mazes,
    /// level navigation, alternative paths, animation state. **Contains no Godot dependency.**
    /// </summary>
    /// <remarks>
    /// Extracted from <c>GameState</c>, which is a Godot autoload <c>Node</c>. Before the
    /// split, `GameState` was 266 lines whose entire Godot surface was `: Node`, `_Ready`,
    /// `_ExitTree`, one `GD.Print` and one `Mathf.Clamp` — roughly 99% plain logic that could
    /// not be tested, because the NUnit suite builds without the Godot SDK.
    ///
    /// `GameState` is now a thin adapter that owns a `MazeSession` and forwards to it, so the
    /// same flows the app runs are exercisable in-process, in milliseconds, with no engine.
    /// This is the humble-object pattern: keep the untestable part as small as it can be.
    ///
    /// WHAT DELIBERATELY STAYED BEHIND
    ///
    /// `PathVisualizationSettings` remains on `GameState`. It is presentation config (27 uses
    /// of Godot's `Color`), and its `DecisionDetailLevel` enum lives in the same file, so
    /// pulling it in here would drag Godot back in and defeat the point. `GameState` resets
    /// those presentation flags itself after delegating the session reset — see
    /// <see cref="ResetVisualizationState"/>.
    /// </remarks>
    public class MazeSession
    {
        /// <summary>Services used to generate and solve. Injected so tests can seed it.</summary>
        public ServiceContainer Services { get; }

        public MazeSession(ServiceContainer? services = null)
        {
            Services = services ?? new ServiceContainer();
        }

        /// <summary>Current maze generation settings.</summary>
        public MazeGenerationSettings Settings { get; set; } = new()
        {
            Size = new MazeSize { X = 20, Y = 20, Z = 1 },
            Algorithm = Maze.Algorithm.GrowingTreeAlgorithm,
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

        /// <summary>Current maze generation results, once generation has run.</summary>
        public MazeGenerationResults? CurrentMaze { get; set; }

        /// <summary>Level being displayed (Z index for 3D mazes).</summary>
        public int CurrentLevel { get; set; }

        /// <summary>Whether dead-end passages are hidden.</summary>
        public bool HideDeadEnds { get; set; }

        /// <summary>Whether the solution path is shown.</summary>
        public bool ShowPath { get; set; }

        /// <summary>Whether the graph representation is shown.</summary>
        public bool ShowGraph { get; set; }

        /// <summary>Whether the abstract graph view is shown.</summary>
        public bool ShowGraphView { get; set; }

        /// <summary>Graph layout for graph view mode.</summary>
        public GraphLayoutType GraphLayout { get; set; } = GraphLayoutType.GridAware;

        #region Alternative paths

        /// <summary>All computed paths for the current maze (index 0 = optimal).</summary>
        public List<PathResult> AllPaths { get; set; } = new();

        /// <summary>Currently selected path index.</summary>
        public int CurrentPathIndex { get; set; }

        /// <summary>Currently selected path, or null when none exist.</summary>
        public PathResult? CurrentPath => AllPaths.Count > 0 ? AllPaths[CurrentPathIndex] : null;

        /// <summary>Whether alternative paths have been computed.</summary>
        public bool AlternativePathsComputed { get; set; }

        /// <summary>Cycles to the next alternative path, wrapping around.</summary>
        public void NextPath()
        {
            if (AllPaths.Count > 1)
            {
                CurrentPathIndex = (CurrentPathIndex + 1) % AllPaths.Count;
            }
        }

        /// <summary>Cycles to the previous alternative path, wrapping around.</summary>
        public void PreviousPath()
        {
            if (AllPaths.Count > 1)
            {
                CurrentPathIndex = (CurrentPathIndex - 1 + AllPaths.Count) % AllPaths.Count;
            }
        }

        #endregion

        #region Animation state

        /// <summary>Whether animation mode is active.</summary>
        public bool IsAnimationMode { get; set; }

        /// <summary>Animation steps for the current path.</summary>
        public List<AlgorithmStep>? AnimationSteps { get; set; }

        /// <summary>Animation playback controller.</summary>
        public AnimationController? AnimationController { get; set; }

        #endregion

        /// <summary>
        /// Clears path and animation state. Callers holding presentation settings should reset
        /// those too — <c>GameState</c> does, after calling this.
        /// </summary>
        public void ResetVisualizationState()
        {
            AllPaths.Clear();
            CurrentPathIndex = 0;
            AlternativePathsComputed = false;
            IsAnimationMode = false;
            AnimationSteps = null;
            AnimationController = null;
        }

        /// <summary>Generates a new maze from the current settings.</summary>
        public MazeGenerationResults GenerateMaze()
        {
            CurrentMaze = Services.MazeGenerationFactory.GenerateMaze(Settings);
            CurrentLevel = 0;
            return CurrentMaze;
        }

        /// <summary>
        /// Loads an imported maze: wraps it for dead-end hiding, computes the shortest path and
        /// graph, and adopts its size into the current settings.
        /// </summary>
        public MazeGenerationResults LoadImportedMaze(IModel model)
        {
            var mazeJumper = Services.MazeFactory.GetMazeJumperFromModel(model);

            // Set up dead-end wrapping so dead-end hiding works.
            mazeJumper.DoDeadEndWrapping(modelBuilder =>
                Services.DeadEndModelWrapperFactory.MakeModel(modelBuilder));

            var shortestPathResult = Services.ShortestPathSolver.GetGraph(mazeJumper);

            // Stats are placeholders: an imported maze has no generation history to report.
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

            Settings.Size = model.Size;
            CurrentLevel = 0;
            ResetVisualizationState();

            return CurrentMaze;
        }

        /// <summary>Sets the displayed level, clamped to the maze's Z range.</summary>
        public void SetLevel(int level)
        {
            if (CurrentMaze != null)
            {
                var maxLevel = Settings.Size.Z - 1;
                // Math.Clamp, not Godot's Mathf.Clamp — this type stays Godot-free.
                CurrentLevel = Math.Clamp(level, 0, maxLevel);
            }
        }

        /// <summary>Moves up one level, if possible.</summary>
        public void NextLevel() => SetLevel(CurrentLevel + 1);

        /// <summary>Moves down one level, if possible.</summary>
        public void PreviousLevel() => SetLevel(CurrentLevel - 1);
    }
}

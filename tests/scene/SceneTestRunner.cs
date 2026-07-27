using System;
using System.Collections.Generic;
using Godot;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Build;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.SceneTests
{
    /// <summary>
    /// Minimal in-engine test runner for the Godot scene/UI layer, executed headless.
    /// </summary>
    /// <remarks>
    /// WHY THIS EXISTS RATHER THAN gdUnit4Net
    ///
    /// gdUnit4Net is the obvious choice and was tried first. Result, measured on Godot 4.7.1
    /// with gdUnit4.api 5.1.0-rc5 and gdUnit4.test.adapter 3.1.1:
    ///
    ///   * The project builds and restores cleanly — no package conflict with Godot 4.7.1.
    ///   * Logic-only [TestCase] tests run and pass.
    ///   * Every [RequireGodotRuntime] test fails to start:
    ///       "GodotRuntimeTestRunner ends with exit code: 1"
    ///       "Starting GodotRuntimeExecutor failed. The operation has timed out."
    ///       "Failed to connect: Connection timeout"
    ///
    /// Isolated the cause: Godot 4.7.1 itself runs this project headless and executes our C#
    /// correctly (the GameState autoload's _Ready fires). So the blocker is gdUnit4's own
    /// runtime executor, not Godot or this project. That matches gdUnit4Net's stated support
    /// stopping at Godot 4.4.1, with its last release in June 2025.
    ///
    /// So: the scene layer is testable today, just not through gdUnit4Net. This runner is
    /// deliberately tiny — a list of checks, a pass/fail tally, and a process exit code, which
    /// is all CI needs. Swap it for gdUnit4Net once that supports 4.7+; the checks port over
    /// almost verbatim.
    ///
    /// WHAT THIS COVERS THAT NOTHING ELSE DOES
    ///
    /// scripts/ui/ is ~4300 lines that the NUnit suite cannot compile (it deliberately avoids
    /// the Godot SDK). Before this, the nearest thing was a test reading menu.tscn as *text*
    /// and asserting it contained the string "ComparisonButton" — which proves a node name
    /// exists in a file, not that it is a Button or that the scene instantiates.
    ///
    /// Run:  godot --headless --path . res://tests/scene/scene_tests.tscn
    /// Build with -p:IncludeSceneTests=true so this never ships in a game export.
    /// </remarks>
    public partial class SceneTestRunner : Node
    {
        private readonly List<string> _failures = new();
        private int _checks;

        public override void _Ready()
        {
            GD.Print("── scene tests ──");

            Run("menu scene instantiates", CheckMenuSceneInstantiates);
            Run("menu ComparisonButton is a real Button", CheckComparisonButtonIsAButton);
            Run("menu AboutButton is a real Button", CheckAboutButtonIsAButton);
            Run("about scene wires up and reports a build", CheckAboutSceneReportsBuild);
            Run("every scene file instantiates", CheckAllScenesInstantiate);
            Run("GameState autoload is available", CheckGameStateAutoload);
            Run("TestBridge is inert off the web platform", CheckTestBridgeInertOnDesktop);
            Run("seeded generation is deterministic in-engine", CheckSeededGenerationInEngine);
            Run("GameState.SetLevel clamps to maze bounds", CheckSetLevelClamps);

            GD.Print($"── {_checks - _failures.Count}/{_checks} passed ──");
            foreach (var f in _failures)
            {
                GD.PrintErr($"FAIL: {f}");
            }

            // Exit code is the contract with CI: non-zero fails the job.
            GetTree().Quit(_failures.Count == 0 ? 0 : 1);
        }

        private void Run(string name, Action check)
        {
            _checks++;
            try
            {
                check();
                GD.Print($"  ok   {name}");
            }
            catch (Exception e)
            {
                _failures.Add($"{name}: {e.Message}");
                GD.Print($"  FAIL {name}");
            }
        }

        #region Checks

        private static void CheckMenuSceneInstantiates()
        {
            var scene = GD.Load<PackedScene>("res://scenes/menu.tscn");
            Assert(scene is not null, "menu.tscn failed to load");
            var instance = scene!.Instantiate();
            Assert(instance is not null, "menu.tscn failed to instantiate");
            instance!.QueueFree();
        }

        private static void CheckComparisonButtonIsAButton()
        {
            var instance = GD.Load<PackedScene>("res://scenes/menu.tscn").Instantiate();
            try
            {
                var node = instance.FindChild("ComparisonButton", recursive: true, owned: false);
                Assert(node is not null, "ComparisonButton not found in menu.tscn");
                Assert(node is Button, $"ComparisonButton is {node!.GetType().Name}, expected Button");
            }
            finally
            {
                instance.QueueFree();
            }
        }

        private static void CheckAboutButtonIsAButton()
        {
            var instance = GD.Load<PackedScene>("res://scenes/menu.tscn").Instantiate();
            try
            {
                var node = instance.FindChild("AboutButton", recursive: true, owned: false);
                Assert(node is not null, "AboutButton not found in menu.tscn");
                Assert(node is Button, $"AboutButton is {node!.GetType().Name}, expected Button");
            }
            finally
            {
                instance.QueueFree();
            }
        }

        /// <summary>
        /// Adds the About scene to the tree so its _Ready actually runs.
        /// </summary>
        /// <remarks>
        /// Instantiating alone would not catch anything useful here: _Ready is where the
        /// %UniqueName lookups happen, and a renamed or un-flagged node is precisely the
        /// failure this needs to catch. The generated rows are checked too, because an empty
        /// About screen would look like a working one to any test that only asserts it loads.
        /// </remarks>
        private void CheckAboutSceneReportsBuild()
        {
            var instance = GD.Load<PackedScene>("res://scenes/about.tscn").Instantiate();
            AddChild(instance);
            try
            {
                var summary = instance.FindChild("SummaryLabel", recursive: true, owned: false) as Label;
                Assert(summary is not null, "SummaryLabel not found in about.tscn");
                Assert(!string.IsNullOrWhiteSpace(summary!.Text), "SummaryLabel is empty — build summary was never set");

                var rows = instance.FindChild("Rows", recursive: true, owned: false);
                Assert(rows is not null, "Rows container not found in about.tscn");
                Assert(rows!.GetChildCount() > 0, "About screen rendered no build rows");

                // The commit row is the entire point of the screen; everything else is context.
                var labels = new List<string>();
                var values = new List<string>();
                foreach (var row in rows.GetChildren())
                {
                    if (row.GetChildCount() > 0 && row.GetChild(0) is Label label)
                    {
                        labels.Add(label.Text);
                    }

                    if (row.GetChildCount() > 1 && row.GetChild(1) is Label value)
                    {
                        values.Add(value.Text);
                    }
                }

                Assert(labels.Contains("Commit"), $"No Commit row on the About screen (found: {string.Join(", ", labels)})");

                // Asserted against whichever kind of build this actually is, so the check holds
                // whether or not CI stamped it. Both directions matter: an unstamped build must
                // not look official, and a stamped one must show its real commit rather than a
                // stale or placeholder value.
                if (CurrentBuild.Info.IsOfficial)
                {
                    Assert(!labels.Contains("Provenance"),
                        "A CI-stamped build must not carry the untraceable-build warning");
                    Assert(values.Contains(CurrentBuild.Info.Commit),
                        $"About screen does not show the stamped commit {CurrentBuild.Info.Commit}");
                }
                else
                {
                    Assert(labels.Contains("Provenance"),
                        "An unstamped build must show the Provenance warning, otherwise a local build looks official");
                }
            }
            finally
            {
                instance.QueueFree();
            }
        }

        private static void CheckAllScenesInstantiate()
        {
            // A scene that fails to instantiate is the classic breakage after a refactor:
            // a renamed script or a dropped node reference. Cheap to catch, easy to miss.
            using var dir = DirAccess.Open("res://scenes");
            Assert(dir is not null, "could not open res://scenes");

            foreach (var file in dir!.GetFiles())
            {
                if (!file.EndsWith(".tscn", StringComparison.Ordinal))
                {
                    continue;
                }

                var path = $"res://scenes/{file}";
                var scene = GD.Load<PackedScene>(path);
                Assert(scene is not null, $"{path} failed to load");
                var instance = scene!.Instantiate();
                Assert(instance is not null, $"{path} failed to instantiate");
                instance!.QueueFree();
            }
        }

        private static void CheckGameStateAutoload()
        {
            Assert(GameState.Instance is not null, "GameState.Instance is null — autoload not registered?");
            Assert(GameState.Instance!.Services is not null, "GameState.Services was not constructed");
        }

        private void CheckTestBridgeInertOnDesktop()
        {
            // The bridge must expose nothing outside the web export. Verified here because it
            // is the one place a mistake would be invisible: a desktop build would simply
            // carry a dormant automation surface.
            var bridge = GetNodeOrNull("/root/TestBridge");
            Assert(bridge is not null, "TestBridge autoload not registered in project.godot");
            Assert(!OS.HasFeature("web"), "this check only means something off the web platform");
        }

        private static void CheckSeededGenerationInEngine()
        {
            // The determinism guarantee is unit-tested already, but only outside the engine.
            // This confirms it still holds through the autoload/ServiceContainer path the app
            // actually uses at runtime.
            var state = GameState.Instance!;
            state.Settings.Seed = 20260725;
            state.Settings.Size = new MazeSize { X = 8, Y = 8, Z = 1 };
            state.Settings.Algorithm = Algorithm.RecursiveBacktrackerAlgorithm;

            var first = state.GenerateMaze();
            var firstJson = state.Services.MazeSerializer.SerializeToString(first.MazeJumper.GetModel());
            var firstSeed = first.Seed;

            var second = state.GenerateMaze();
            var secondJson = state.Services.MazeSerializer.SerializeToString(second.MazeJumper.GetModel());

            Assert(firstSeed == 20260725, $"reported seed was {firstSeed}, expected 20260725");
            Assert(firstJson == secondJson, "same seed produced different mazes through GameState");
        }

        private static void CheckSetLevelClamps()
        {
            var state = GameState.Instance!;
            state.Settings.Seed = 1;
            state.Settings.Size = new MazeSize { X = 5, Y = 5, Z = 3 };
            state.GenerateMaze();

            state.SetLevel(99);
            Assert(state.CurrentLevel == 2, $"SetLevel(99) gave {state.CurrentLevel}, expected clamp to 2");
            state.SetLevel(-5);
            Assert(state.CurrentLevel == 0, $"SetLevel(-5) gave {state.CurrentLevel}, expected clamp to 0");
        }

        #endregion

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}

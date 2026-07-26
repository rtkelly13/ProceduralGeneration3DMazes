using System;
using System.Globalization;
using System.Text;
using Godot;
using ProceduralMaze.Build;
using ProceduralMaze.Autoload;
using ProceduralMaze.Maze;
using ProceduralMaze.Maze.Agents;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Testing
{
    /// <summary>
    /// Test automation bridge for the web build. Lets Playwright drive and observe the app.
    /// </summary>
    /// <remarks>
    /// WHY THIS EXISTS
    ///
    /// A Godot web export renders everything into a single &lt;canvas&gt;. There are no DOM
    /// nodes for buttons, menus or labels, so Playwright's locator model
    /// (getByRole/getByText/toBeVisible) cannot see anything inside the app. Playwright can
    /// *drive* it — real keyboard and mouse events reach the engine — but without a hook it
    /// cannot *assert* anything beyond "a canvas exists" and raw pixels.
    ///
    /// This bridge supplies the missing observation channel, and a command channel so tests
    /// can set up deterministic state instead of clicking pixel coordinates.
    ///
    /// CONTRACT (see docs/TEST_BRIDGE.md for the full reference)
    ///
    ///   window.__mazeTestApi   "1" once the bridge is live (absent = disabled)
    ///   window.__mazeState     JSON string, republished whenever it changes
    ///   window.__mazeCommand   function(jsonString) — fire-and-forget command
    ///
    /// DESIGN CONSTRAINTS, all deliberate:
    ///
    /// * Opt-in only. Disabled unless the URL carries `test=1` (or a `seed` parameter, which
    ///   implies automation). Normal visitors never get the API surface.
    /// * Web-only. JavaScriptBridge is implemented only in the Web export, so every entry
    ///   point is guarded by OS.HasFeature("web") and the bridge no-ops on desktop.
    /// * No JavaScriptBridge.Eval. The docs note eval "may be disabled in custom export
    ///   templates for security reasons" and this project builds with a patched template
    ///   (see docs/WEB_EXPORT.md), so relying on eval would be a coin flip. GetInterface plus
    ///   property assignment avoids that path entirely.
    /// * Commands are fire-and-forget. Godot's docs do not state that a callback's return
    ///   value reaches the JavaScript caller, and the examples never return one. Rather than
    ///   depend on unspecified behaviour, commands return nothing and tests observe the
    ///   result via __mazeState. Callbacks also must take exactly one Array argument or they
    ///   are never invoked, which this satisfies.
    /// * JSON is hand-built, not serialized via reflection. The web build is trimmed
    ///   (see the csproj's TrimmerRootAssembly entries); reflection-based serializers are
    ///   exactly what trimming breaks, and it would fail only on web. A StringBuilder cannot
    ///   be trimmed away.
    /// * Invariant culture everywhere. The web build forces InvariantGlobalization, so any
    ///   culture-dependent number formatting would diverge between desktop and web.
    /// </remarks>
    public partial class TestBridge : Node
    {
        /// <summary>Query parameter that switches the bridge on.</summary>
        private const string EnableParam = "test";

        private JavaScriptObject? _window;
        private JavaScriptObject? _commandCallback;
        private string _lastPublishedState = "";
        private string _lastError = "";
        private string _lastCommand = "";
        private bool _enabled;

        public override void _Ready()
        {
            // JavaScriptBridge exists only in the web export. On desktop this node is inert,
            // which keeps the editor and desktop builds completely unaffected.
            if (!OS.HasFeature("web"))
            {
                return;
            }

            _window = JavaScriptBridge.GetInterface("window");
            if (_window is null)
            {
                GD.PushWarning("TestBridge: window interface unavailable; bridge disabled.");
                return;
            }

            var query = ReadQueryString();
            if (!ShouldEnable(query))
            {
                return;
            }

            _enabled = true;

            // One command entry point rather than a callback per command: the one-Array-arg
            // rule applies to each callback, so a single JSON-string entry point is both
            // simpler and easier to version.
            _commandCallback = JavaScriptBridge.CreateCallback(Callable.From<Godot.Collections.Array>(OnCommand));
            _window.Set("__mazeCommand", _commandCallback);
            _window.Set("__mazeTestApi", "1");

            ApplyStartupParameters(query);
            PublishState(force: true);

            GD.Print("TestBridge: enabled (window.__mazeCommand / window.__mazeState)");
        }

        public override void _Process(double delta)
        {
            if (_enabled)
            {
                // Republish only on change: assigning across the JS boundary every frame is
                // needless work, and a churning value is harder for a test to wait on.
                PublishState(force: false);
            }
        }

        /// <summary>Enabled by `?test=1`, or by any `seed=` parameter (which implies automation).</summary>
        private static bool ShouldEnable(string query)
        {
            return TestBridgeProtocol.GetParam(query, EnableParam) == "1" || TestBridgeProtocol.GetParam(query, "seed") is not null;
        }

        private string ReadQueryString()
        {
            try
            {
                var location = _window?.Get("location").As<JavaScriptObject>();
                return location?.Get("search").AsString() ?? "";
            }
            catch (Exception e)
            {
                // Never let a bridge problem break the app for a real visitor.
                GD.PushWarning($"TestBridge: could not read location.search: {e.Message}");
                return "";
            }
        }

        /// <summary>
        /// Applies generation parameters supplied on the URL and generates immediately.
        /// This is what makes a screenshot reproducible: same URL, same maze.
        /// </summary>
        private void ApplyStartupParameters(string query)
        {
            var seed = TestBridgeProtocol.GetIntParam(query, "seed");
            if (seed is null)
            {
                return;
            }

            var settings = GameState.Instance?.Settings;
            if (settings is null)
            {
                _lastError = "GameState not ready";
                return;
            }

            settings.Seed = seed;
            settings.Size = new MazeSize
            {
                X = TestBridgeProtocol.GetIntParam(query, "x") ?? settings.Size.X,
                Y = TestBridgeProtocol.GetIntParam(query, "y") ?? settings.Size.Y,
                Z = TestBridgeProtocol.GetIntParam(query, "z") ?? settings.Size.Z,
            };

            var algorithm = TestBridgeProtocol.ParseAlgorithm(TestBridgeProtocol.GetParam(query, "algorithm"));
            if (algorithm is not null)
            {
                settings.Algorithm = algorithm.Value;
            }

            GenerateAndShow();
        }

        private void OnCommand(Godot.Collections.Array args)
        {
            if (args.Count == 0)
            {
                return;
            }

            var raw = args[0].AsString();
            _lastCommand = raw;
            _lastError = "";

            try
            {
                // Minimal field extraction rather than a JSON parser: the command surface is
                // tiny and fixed, and this keeps the trimmed web build free of reflection.
                var cmd = TestBridgeProtocol.GetJsonString(raw, "cmd");
                switch (cmd)
                {
                    case "generate":
                        HandleGenerate(raw);
                        break;
                    case "setLevel":
                        GameState.Instance?.SetLevel(TestBridgeProtocol.GetJsonInt(raw, "level") ?? 0);
                        break;
                    case "showPath":
                        if (GameState.Instance is not null)
                        {
                            GameState.Instance.ShowPath = TestBridgeProtocol.GetJsonBool(raw, "value") ?? true;
                        }
                        break;
                    case "nextPath":
                        GameState.Instance?.NextPath();
                        break;
                    case "previousPath":
                        GameState.Instance?.PreviousPath();
                        break;
                    case "goto":
                        ChangeScene(TestBridgeProtocol.GetJsonString(raw, "scene"));
                        break;
                    default:
                        _lastError = $"unknown command '{cmd}'";
                        break;
                }
            }
            catch (Exception e)
            {
                // Surface failures through state instead of throwing into the JS caller,
                // which would be invisible to the test.
                _lastError = $"{e.GetType().Name}: {e.Message}";
            }

            PublishState(force: true);
        }

        private void HandleGenerate(string raw)
        {
            var settings = GameState.Instance?.Settings;
            if (settings is null)
            {
                _lastError = "GameState not ready";
                return;
            }

            var seed = TestBridgeProtocol.GetJsonInt(raw, "seed");
            if (seed is not null)
            {
                settings.Seed = seed;
            }

            settings.Size = new MazeSize
            {
                X = TestBridgeProtocol.GetJsonInt(raw, "x") ?? settings.Size.X,
                Y = TestBridgeProtocol.GetJsonInt(raw, "y") ?? settings.Size.Y,
                Z = TestBridgeProtocol.GetJsonInt(raw, "z") ?? settings.Size.Z,
            };

            var algorithm = TestBridgeProtocol.ParseAlgorithm(TestBridgeProtocol.GetJsonString(raw, "algorithm"));
            if (algorithm is not null)
            {
                settings.Algorithm = algorithm.Value;
            }

            GenerateAndShow();
        }

        private void GenerateAndShow()
        {
            GameState.Instance?.GenerateMaze();
            ChangeScene("maze");
        }

        private void ChangeScene(string? scene)
        {
            var path = TestBridgeProtocol.ResolveScenePath(scene);

            if (path is null)
            {
                _lastError = $"unknown scene '{scene}'";
                return;
            }

            // Deferred: changing scene during a callback frees the node currently executing.
            CallDeferred(nameof(DoChangeScene), path);
        }

        private void DoChangeScene(string path)
        {
            var error = GetTree().ChangeSceneToFile(path);
            if (error != Error.Ok)
            {
                _lastError = $"scene change failed: {error}";
            }
        }

        #region State publishing

        private void PublishState(bool force)
        {
            var json = BuildStateJson();
            if (!force && json == _lastPublishedState)
            {
                return;
            }

            _lastPublishedState = json;
            _window?.Set("__mazeState", json);
        }

        private string BuildStateJson()
        {
            var state = GameState.Instance;
            var maze = state?.CurrentMaze;
            var jumper = maze?.MazeJumper;

            var sb = new StringBuilder(512);
            sb.Append('{');
            TestBridgeProtocol.AppendBool(sb, "ready", state is not null);
            sb.Append(',');
            TestBridgeProtocol.AppendString(sb, "scene", GetTree()?.CurrentScene?.SceneFilePath ?? "");
            sb.Append(',');
            TestBridgeProtocol.AppendBool(sb, "hasMaze", maze is not null);
            sb.Append(',');
            // Build identity, so an automated check can confirm WHICH build it is talking to.
            // Published unconditionally: unlike the maze fields it does not depend on any state
            // existing, and a smoke test needs it before the app has done anything.
            TestBridgeProtocol.AppendString(sb, "buildCommit", CurrentBuild.Info.Commit);
            sb.Append(',');
            TestBridgeProtocol.AppendString(sb, "buildBranch", CurrentBuild.Info.Branch);
            sb.Append(',');
            TestBridgeProtocol.AppendString(sb, "buildTime", CurrentBuild.Info.TimestampUtc);
            sb.Append(',');
            TestBridgeProtocol.AppendString(sb, "buildRunId", CurrentBuild.Info.RunId);

            if (state is not null)
            {
                sb.Append(',');
                TestBridgeProtocol.AppendString(sb, "algorithm", state.Settings.Algorithm.ToString());
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "sizeX", state.Settings.Size.X);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "sizeY", state.Settings.Size.Y);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "sizeZ", state.Settings.Size.Z);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "currentLevel", state.CurrentLevel);
                sb.Append(',');
                TestBridgeProtocol.AppendBool(sb, "showPath", state.ShowPath);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "pathCount", state.AllPaths.Count);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "currentPathIndex", state.CurrentPathIndex);
            }

            if (maze is not null)
            {
                sb.Append(',');
                // The seed that actually produced this maze — the value to feed back in to
                // reproduce it. See docs/REGRESSION_TESTING.md.
                TestBridgeProtocol.AppendInt(sb, "seed", maze.Seed);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "shortestPath", maze.HeuristicsResults?.ShortestPathResult?.ShortestPath ?? -1);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "totalCells", maze.HeuristicsResults?.TotalCells ?? -1);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "deadEnds", maze.Metrics?.DeadEnds ?? -1);
                sb.Append(',');
                TestBridgeProtocol.AppendInt(sb, "junctions", maze.Metrics?.Junctions ?? -1);
            }

            if (jumper is not null)
            {
                sb.Append(',');
                AppendPoint(sb, "start", jumper.StartPoint);
                sb.Append(',');
                AppendPoint(sb, "end", jumper.EndPoint);
            }

            sb.Append(',');
            TestBridgeProtocol.AppendString(sb, "lastCommand", _lastCommand);
            sb.Append(',');
            TestBridgeProtocol.AppendString(sb, "lastError", _lastError);
            sb.Append('}');
            return sb.ToString();
        }

        private static void AppendPoint(StringBuilder sb, string name, MazePoint p)
        {
            sb.Append('"').Append(name).Append("\":{");
            TestBridgeProtocol.AppendInt(sb, "x", p.X);
            sb.Append(',');
            TestBridgeProtocol.AppendInt(sb, "y", p.Y);
            sb.Append(',');
            TestBridgeProtocol.AppendInt(sb, "z", p.Z);
            sb.Append('}');
        }

        #endregion

    }
}

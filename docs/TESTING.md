# Testing Map

Which layer to test where, and why. Start here before adding a test.

| Layer | Where | Runs | Covers | Needs |
|---|---|---|---|---|
| **Unit + integration** | `tests/*.cs` (NUnit) | every PR, ~10s | maze logic, serialization, solvers, agents, bridge wire format, **session flows, animation playback** | .NET only — **no Godot SDK** |
| **Scene / UI** | `tests/scene/` (in-engine) | every PR, ~1min | scenes instantiate, node types, autoloads, runtime wiring | Godot binary, headless |
| **Functional (browser)** | `tests/visual/functional.spec.ts` | post-deploy | app state through the real WASM runtime | deployed build + test bridge |
| **Visual** | `tests/visual/maze.spec.ts` | post-deploy | rendered output at fixed seeds | deployed build + test bridge |
| **Performance** | `benchmarks/` | manual | algorithm throughput | .NET only |

Rule of thumb: **push tests down.** The unit layer is ~10s and needs nothing; the browser
layer needs a Windows-built web export and a Vercel deploy. Only test in the browser what
*only the browser can break*.

## Unit tests — `dotnet test tests/`

550 tests. The project deliberately compiles **without the Godot SDK**, which is what keeps it
fast and portable. Any code needing `using Godot;` cannot live here — that is the boundary, and
it is why the scene layer exists.

### Parallelism: fixtures are one instance per test

The suite runs `[assembly: Parallelizable(ParallelScope.All)]` **and**
`[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]`
([`tests/TestSetup.cs`](../tests/TestSetup.cs)). The second is not optional.

With NUnit's default `SingleInstance`, `ParallelScope.All` runs a fixture's test cases
concurrently **against a single fixture instance**, so every field assigned in `[SetUp]` is a
data race between sibling tests. It cost real time to diagnose: two `RandomValueTests` failures
on CI, `NullReferenceException` on `point.X`, passing on every local run and on a re-run of the
same commit. The cause was a `[SetUp]` that re-reads its own mock field *after* configuring it,
so a sibling's fresh-but-unconfigured mock could be captured instead; a loose Moq mock returns
`null` for a reference type, and the null then surfaced far from its origin.

Two things worth keeping:

- **`InstancePerTestCase` is the fix, not `[NonParallelizable]`.** Marking the fixture serial
  hides one instance of a whole-assembly problem — `MovementHelperTests` carried exactly that
  workaround. A guard test now fails by name if the attribute is removed, because the natural
  symptom is an occasional unexplained flake.
- **Don't debug concurrency with `Console.WriteLine`.** NUnit captures output per test and
  replays it attached to that test's result, which *reorders it into a false sequence* — the
  first pass at this looked like proof that tests within a fixture ran serially. Append to a
  file with a timestamp and thread id instead.

Any `[OneTimeSetUp]`/`[OneTimeTearDown]` added later must be `static` under this lifecycle.

## Segregating behaviour from Godot

The single highest-leverage thing for testability, and mostly already true of this codebase.
Measured Godot-coupling density in `scripts/ui/`:

| File | Lines | Godot refs | |
|---|---|---|---|
| `AnimationController.cs` | 254 | **0** | no `using Godot;` |
| `MazeImportExport.cs` | 363 | 9 | only `FileAccess`, `OS` |
| `ImportExportResult.cs` | 54 | **0** | |
| `PathVisualizationSettings.cs` | 206 | 27 | all one type: `Color` |
| `MazeMain.cs` | 1473 | 156 (11%) | `Input.` ×28, `GetNode` ×24, `AddChild` ×13 |
| `GraphViewRenderer.cs` | 631 | 73 (12%) | genuine drawing |

Three tiers, and the plan follows from them:

1. **Already Godot-free** — just include it in the test project. `AnimationController` and
   `ImportExportResult` compile there with no changes at all.
2. **Trivially freeable** — `GameState` (done, below), `MazeImportExport` (abstract `FileAccess`
   and `OS`), `PathVisualizationSettings` (swap `Color` for a plain RGBA struct).
3. **Genuinely Godot** — `GraphViewRenderer`'s drawing, `MazeMain`'s input and node wiring,
   scene lifecycle. **Don't extract these.** Render code's contract *is* the pixels, which is
   what visual tests are for; chasing coverage here produces anaemic wrappers and nothing else.

### `MazeSession`: the humble-object split

`GameState` was 266 lines whose **entire** Godot surface was `: Node`, `_Ready`, `_ExitTree`,
one `GD.Print` and one `Mathf.Clamp` — nearly all logic, none of it testable.

It is now a thin adapter over [`MazeSession`](../scripts/session/MazeSession.cs), which holds
the state and operations and has no Godot dependency. The public API is unchanged, so nothing
in `scripts/ui` needed editing. `PathVisualizationSettings` deliberately stayed on the node:
it is presentation config built on `Color`, and its `DecisionDetailLevel` enum lives in the
same file, so moving it would drag Godot straight back in.

The payoff is *flow* tests that were previously impossible — generate → navigate levels →
cycle paths → import → regenerate, in-process, in milliseconds. See
[`tests/MazeSessionTests.cs`](../tests/MazeSessionTests.cs).

**It found a real bug on its first run.** `GameState.LoadImportedMaze` applies dead-end
wrapping and then builds a graph, and that combination crashed with
`Nullable object must have a value` — so **importing any maze crashed the app**. A dead-end
cell has exactly one direction (the one you arrived from), so `GraphBuilder`'s corridor walk
treated it as neither junction nor terminus and dereferenced null. Dead-end wrapping *creates*
such cells by hiding passages. Several existing tests did wrapping, and several did graph
building; none did both, which is how it survived. Fixed, with regression coverage in
[`tests/GraphBuilderDeadEndTests.cs`](../tests/GraphBuilderDeadEndTests.cs).

That is the argument for this refactor in one incident: the bug was always reachable from the
UI, and became visible the moment the flow was testable without the engine.

See [REGRESSION_TESTING.md](./REGRESSION_TESTING.md) for the determinism guarantees the suite
relies on and the golden-file plan.

## Scene tests — headless Godot

```sh
dotnet build ProceduralGeneration3DMazes.csproj -c Debug -p:IncludeSceneTests=true
godot --headless --path . res://tests/scene/scene_tests.tscn   # exit code 0 = pass
```

**What this closed:** `scripts/ui/` is ~4300 lines that the NUnit suite cannot compile. The
nearest thing to coverage was a test that read `menu.tscn` as **text** and asserted it
contained the string `"ComparisonButton"` — proving a node name appears in a file, not that it
is a `Button` or that the scene even instantiates. The scene runner checks the real thing.

Current checks: every scene instantiates, `ComparisonButton` is genuinely a `Button`, the
`GameState` autoload initialises its `ServiceContainer`, the test bridge is inert off the web
platform, seeded generation is deterministic *through the autoload path the app actually
uses*, and `SetLevel` clamps to maze bounds.

The runner ([`tests/scene/SceneTestRunner.cs`](../tests/scene/SceneTestRunner.cs)) is
deliberately ~200 lines: a list of checks, a tally, and a process exit code. It is compiled
only under `-p:IncludeSceneTests=true`, so it never ships in a game export.

### Why not gdUnit4Net

[gdUnit4Net](https://github.com/godot-gdunit-labs/gdUnit4Net) is the obvious choice and was
tried first. Measured on **Godot 4.7.1** with `gdUnit4.api 5.1.0-rc5` and
`gdUnit4.test.adapter 3.1.1`:

| Result | |
|---|---|
| Package restore and build against Godot.NET.Sdk 4.7.1 | ✅ no conflict |
| Logic-only `[TestCase]` tests | ✅ run and pass |
| Every `[RequireGodotRuntime]` test | ❌ `GodotRuntimeTestRunner ends with exit code: 1`, `Starting GodotRuntimeExecutor failed. The operation has timed out.`, `Failed to connect: Connection timeout` |

Isolated the cause: **Godot 4.7.1 runs this project headless and executes its C# correctly**
(the `GameState` autoload's `_Ready` fires and prints). So the blocker is gdUnit4's own runtime
executor, not Godot and not this project. Consistent with gdUnit4Net's stated support stopping
at **Godot 4.4.1**, last release **June 2025**.

Setting `<GodotProjectDir>` at the real project root and pre-importing the project did not
change the outcome.

**So:** the scene layer is testable today, just not through gdUnit4Net. Revisit when it
supports 4.7+ — the checks port over nearly verbatim (`AssertThat(node).IsInstanceOf<Button>()`
instead of a local `Assert`), and the custom runner can then be deleted. Until then, a
200-line runner that demonstrably works beats a framework that does not.

## Functional tests — Playwright + test bridge

```sh
cd tests/visual && npm ci
MAZE_URL=https://… MAZE_TEST_BRIDGE=1 npx playwright test --project=functional
```

Asserts on **application state**, not pixels. Possible only because of the in-app bridge — a
Godot web export is one `<canvas>`, so Playwright's locators see nothing inside the app.
Contract and design rationale: **[TEST_BRIDGE.md](./TEST_BRIDGE.md)**.

Playwright *can* drive input with no app changes (real key/mouse events reach the engine);
what needed the bridge was **observation**.

Scope kept deliberately small — URL seeding works, the same seed reproduces a maze in the
browser, different seeds differ, the maze is solvable, level clamping, scene navigation, bad
commands are reported not swallowed, and keyboard input reaches the engine without a crash.
Everything else about the UI belongs in scene tests, which are ~60× cheaper to run.

## Visual regression — Playwright screenshots

See **[VISUAL_REGRESSION.md](./VISUAL_REGRESSION.md)**. Depends on the same bridge for
deterministic seeding, plus committed baseline PNGs.

## CI layout

| Workflow | Job | Trigger |
|---|---|---|
| `test.yml` | `test` | every PR |
| `test.yml` | `scene-tests` | every PR |
| `test.yml` | `visual-harness` | every PR |
| `web-export.yml` | `smoke-*` → `visual-*` (functional + visual) | merge to `main`, or `/preview` |

Both browser suites are **gated on `MAZE_TEST_BRIDGE=1`** and currently skip. The bridge is
unit- and scene-tested, but whether it behaves under the *patched* web export template is
unproven until a deploy exists to check. Skipping beats a false red — see
[TEST_BRIDGE.md → Enabling in CI](./TEST_BRIDGE.md#enabling-in-ci).

The scene-test job downloads the **official upstream** Linux editor, checksum-pinned in
[`.github/editor-checksums.txt`](../.github/editor-checksums.txt) exactly like the patched
Windows one. The patched fork is only needed for the *web export*; plain Godot runs in-engine
tests fine.

## Known gaps

- **The patched-template question** above: the single biggest unknown.
- **`scripts/ui/` is still thinly covered.** `AnimationController` is now fully covered and
  `GameState`'s logic moved to `MazeSession`, but the interaction logic inside `MazeMain`
  (1473 lines) is largely untested. Next: abstract `FileAccess`/`OS` in `MazeImportExport` so
  import/export round-trips become testable, then pull orchestration out of `MazeMain`
  incrementally as it is touched — not as a big-bang rewrite.
- **`PerfectAgent` is worst-case exponential** — see
  [REGRESSION_TESTING.md](./REGRESSION_TESTING.md). Bounded in tests, unfixed in the app.
- **No golden files yet.** Designed in REGRESSION_TESTING.md, not built.

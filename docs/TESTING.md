# Testing Map

Which layer to test where, and why. Start here before adding a test.

| Layer | Where | Runs | Covers | Needs |
|---|---|---|---|---|
| **Unit** | `tests/*.cs` (NUnit) | every PR, ~10s | maze logic, serialization, solvers, agents, bridge wire format | .NET only — **no Godot SDK** |
| **Scene / UI** | `tests/scene/` (in-engine) | every PR, ~1min | scenes instantiate, node types, autoloads, runtime wiring | Godot binary, headless |
| **Functional (browser)** | `tests/visual/functional.spec.ts` | post-deploy | app state through the real WASM runtime | deployed build + test bridge |
| **Visual** | `tests/visual/maze.spec.ts` | post-deploy | rendered output at fixed seeds | deployed build + test bridge |
| **Performance** | `benchmarks/` | manual | algorithm throughput | .NET only |

Rule of thumb: **push tests down.** The unit layer is ~10s and needs nothing; the browser
layer needs a Windows-built web export and a Vercel deploy. Only test in the browser what
*only the browser can break*.

## Unit tests — `dotnet test tests/`

493 tests. The project deliberately compiles `scripts/maze/**` plus `ServiceContainer` and
`TestBridgeProtocol` **without the Godot SDK**, which is what keeps it fast and portable. Any
code that needs `using Godot;` cannot be tested here — that is the boundary, and it is why the
scene layer exists.

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
- **`scripts/ui/` is still thinly covered.** Scene tests establish the harness and cover
  wiring; the interaction logic inside `MazeMain` (1473 lines) is largely untested. That is
  the next place to spend effort, and it is now cheap to do.
- **`PerfectAgent` is worst-case exponential** — see
  [REGRESSION_TESTING.md](./REGRESSION_TESTING.md). Bounded in tests, unfixed in the app.
- **No golden files yet.** Designed in REGRESSION_TESTING.md, not built.

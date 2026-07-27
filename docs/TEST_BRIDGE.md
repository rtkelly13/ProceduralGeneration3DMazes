# Test Bridge (web automation API)

The in-app hook that makes browser-based testing of this project possible.
Implementation: [`scripts/testing/TestBridge.cs`](../scripts/testing/TestBridge.cs) and
[`scripts/testing/TestBridgeProtocol.cs`](../scripts/testing/TestBridgeProtocol.cs).

## Why it has to exist

A Godot web export renders **everything into a single `<canvas>`**. Menus, buttons and labels
are drawn inside it, so there are no DOM nodes to find. Playwright's whole locator model —
`getByRole`, `getByText`, `toBeVisible` — sees nothing but one canvas element.

Playwright *can* drive the app with no changes at all: real keyboard and mouse events reach
the engine. What it cannot do is **observe** anything. Without a hook, the only available
assertions are "a canvas exists" and pixel comparison.

This bridge supplies the missing observation channel, plus a command channel so tests can set
up state directly instead of clicking pixel coordinates.

## Contract

| Property | Type | Meaning |
|---|---|---|
| `window.__mazeTestApi` | `"1"` | Bridge is live. **Absent when disabled** — check this first. |
| `window.__mazeState` | JSON **string** | Current app state, republished whenever it changes. |
| `window.__mazeCommand` | `function(jsonString)` | Fire-and-forget command. Returns nothing. |

### Enabling it

Disabled by default. It activates only when the URL carries either:

- `?test=1` — explicit, or
- any `?seed=…` — implies automation, and is what the visual suite uses.

Normal visitors never receive the API surface.

### State fields

```jsonc
{
  "ready": true,                  // GameState initialised
  "scene": "res://scenes/maze.tscn",
  "hasMaze": true,
  "algorithm": "RecursiveBacktrackerAlgorithm",
  "sizeX": 10, "sizeY": 10, "sizeZ": 1,
  "currentLevel": 0,
  "showPath": false,
  "pathCount": 0,
  "currentPathIndex": 0,
  "seed": 20260725,               // the seed that produced this maze — replay it to reproduce
  "shortestPath": 42,
  "totalCells": 100,
  "deadEnds": 17,
  "junctions": 9,
  "start": { "x": 0, "y": 0, "z": 0 },
  "end":   { "x": 9, "y": 9, "z": 0 },
  "buildCommit": "12f29bd…",       // which build this is — see BUILD_VERIFICATION.md
  "buildBranch": "main",
  "buildTime": "2026-07-26T09:15:03Z",
  "buildRunId": "30194540175",
  "lastCommand": "{\"cmd\":\"generate\"}",
  "lastError": ""                 // non-empty when a command failed
}
```

`seed`, `shortestPath`, `totalCells`, `deadEnds`, `junctions`, `start` and `end` appear only
once a maze exists (`hasMaze: true`).

The `build*` fields are always present — they do not depend on any app state, and a check that
wants to know *which build it is talking to* needs them before the app has done anything. They
are empty on an unstamped local build. See [BUILD_VERIFICATION.md](./BUILD_VERIFICATION.md).

### Commands

| Command | Fields | Effect |
|---|---|---|
| `generate` | `seed?`, `algorithm?`, `x?`, `y?`, `z?` | Generates and shows a maze. Omitted fields keep their current value. |
| `setLevel` | `level` | Changes the displayed Z level (clamped to the maze). |
| `showPath` | `value` | Toggles solution-path display. |
| `nextPath` / `previousPath` | — | Cycles alternative paths. |
| `goto` | `scene`: `maze`\|`menu`\|`comparison`\|`loader`\|`about` | Changes screen. |

`algorithm` accepts `backtracker` (or `recursivebacktracker`), `growingtree`, `binarytree`,
`prims` — case-insensitive.

### Usage from Playwright

```ts
await page.goto(`${MAZE_URL}/?test=1&seed=20260725&algorithm=backtracker&x=10&y=10&z=1`);
await page.waitForFunction(() => window.__mazeTestApi === "1");
await page.waitForFunction(() => JSON.parse(window.__mazeState).hasMaze === true);

const state = JSON.parse(await page.evaluate(() => window.__mazeState));
expect(state.seed).toBe(20260725);

// Commands are fire-and-forget — observe the result through state.
await page.evaluate(() => window.__mazeCommand(JSON.stringify({ cmd: "setLevel", level: 2 })));
await expect.poll(async () => (await readState(page)).currentLevel).toBe(2);
```

Working examples: [`tests/visual/functional.spec.ts`](../tests/visual/functional.spec.ts).

## Design decisions, and why

Each of these is a constraint discovered in Godot's documentation or this project's setup, not
a preference.

**Commands are fire-and-forget.** Godot's docs never state that a `create_callback` callback's
return value reaches the JavaScript caller, and none of the examples return one. Rather than
depend on unspecified behaviour, commands return nothing and tests observe results via
`__mazeState`. Callbacks must also take **exactly one `Array` argument** or they are never
invoked — a single JSON-string entry point satisfies that naturally.

**No `JavaScriptBridge.Eval`.** The docs note eval "may be disabled in custom export templates
for security reasons", and this project builds with a **patched** template (see
[WEB_EXPORT.md](./WEB_EXPORT.md)). Relying on eval would be a coin flip. `GetInterface("window")`
plus property assignment avoids that path entirely.

**One JSON string, not nested objects.** Only base types (int, float, string, bool) convert
automatically across the boundary. Publishing a single string sidesteps object marshalling
completely, and `JSON.parse` on the test side is trivial.

**Hand-rolled JSON, no `System.Text.Json`.** The web build is **trimmed** (see the
`TrimmerRootAssembly` entries in the csproj). Reflection-based serialization is exactly what
trimming breaks — and it would fail *only in the browser*, the most expensive place to find a
bug. A `StringBuilder` cannot be trimmed away.

**Invariant culture everywhere.** The web build forces `InvariantGlobalization`; culture-
dependent number formatting would silently diverge between desktop and web.

**Protocol split from the node.** `TestBridgeProtocol` is Godot-free so the ordinary NUnit
suite can cover it — see [`tests/TestBridgeProtocolTests.cs`](../tests/TestBridgeProtocolTests.cs)
(54 tests). `TestBridge.cs` is a Godot `Node` and can only run inside the engine, so all the
fiddly parsing lives in the part that is cheaply testable.

**Web-only and inert elsewhere.** `JavaScriptBridge` exists only in the web export, so every
entry point is guarded by `OS.HasFeature("web")`. The scene tests assert the bridge is inert
off the web platform, because a dormant automation surface in a desktop build is exactly the
kind of mistake nothing else would notice.

## Verification status

| Aspect | How verified |
|---|---|
| Wire format (parsers, JSON escaping, round-trip) | ✅ 54 NUnit tests, run on every PR |
| Compiles against the real GodotSharp 4.7.1 API | ✅ `dotnet build` of the Godot project |
| Autoload registered; inert off web | ✅ headless scene test in Godot 4.7.1 |
| Behaviour inside the **patched web export** | ✅ **confirmed on a real deploy** |

That last row was the project's longest-standing unknown, and it is now closed. On a preview
deploy of the patched export (`workflow_dispatch`, run `30216793098`):

```
✅ TEST BRIDGE PRESENT
   window.__mazeState: {"ready":true,"scene":"res://scenes/menu.tscn","hasMaze":false,
                        "buildCommit":"b3995abd…","buildBranch":"claude/godot-wasm-hosting-eval-3a6h5m",
                        "buildTime":"2026-07-26T19:25:05Z","buildRunId":"30216793098",
                        "algorithm":"GrowingTreeAlgorithm","sizeX":20,"sizeY":20,…}
```

So `JavaScriptBridge.GetInterface("window")` plus property assignment works normally under the
patched template, `CreateCallback` is installed, and state publishes. `MAZE_TEST_BRIDGE=1` is
now set in CI and the functional suite runs on every deploy.

The visual suite still skips, but on `MAZE_VISUAL_BASELINES` and for an unrelated reason: no
baseline PNGs are committed yet. See [VISUAL_REGRESSION.md](./VISUAL_REGRESSION.md).

## Enabling in CI

**Already enabled.** `MAZE_TEST_BRIDGE: "1"` is set on the functional step of both the
`visual-production` and `visual-preview` jobs, on the strength of the deploy evidence above.

The smoke test still reports bridge presence on every deploy, which is how a regression would be
noticed — if a future editor or template bump broke `JavaScriptBridge`, the functional suite would
go red and this line would say why:

```
✅ TEST BRIDGE PRESENT — set MAZE_TEST_BRIDGE=1 to enable the browser suites
⚠️  TEST BRIDGE ABSENT — window.__mazeTestApi never became "1".
```

Reported, not asserted: the bridge is an automation aid, and a build without it is still a
working build, so failing a deploy over it would block releases for no user-visible reason. But
it is the one thing unit and scene tests cannot answer, so it is checked automatically instead
of waiting on someone to look.

If `__mazeTestApi` ever goes absent again, check the Godot console output for the
`TestBridge: enabled` line — its absence means either the URL flag was missing or
`GetInterface("window")` returned null under the patched template.

To reproduce the check by hand against any deploy:

```js
window.__mazeTestApi === "1"          // with ?test=1 in the URL
typeof window.__mazeCommand === "function"
JSON.parse(window.__mazeState).ready === true
```

## Security notes

- **Off by default.** No `test=1`/`seed` parameter, no API surface.
- **Read/command only.** The bridge exposes generation and navigation; it cannot read files,
  reach the network, or execute arbitrary code. There is no `eval` path in either direction.
- **It does ship in the export.** The code is present in the deployed build, just dormant.
  That is a deliberate trade: testing the *real* artefact matters more than shaving a
  dormant class, and a test-only build would no longer be the thing users run. If that trade
  ever stops being acceptable, gate the compile behind a define rather than at runtime.

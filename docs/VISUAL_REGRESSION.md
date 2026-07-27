# Visual Regression Testing (web build)

Catches rendering regressions in the browser build that no C# test can see: a shader or
material change, a camera/viewport shift, a UI theme break, a canvas that boots but draws
nothing.

**Tooling:** [`@playwright/test`](https://playwright.dev/docs/test-snapshots) and its built-in
`toHaveScreenshot()` snapshot comparison — the standard for web visual regression. The repo
already uses Playwright for the post-deploy smoke test, so this adds no new tool.

**Status:** harness built and self-verified. The maze suite is **skipped** until the web
build supports URL-parameter seeding (below) — deliberately skipped rather than failing, so
it can't report a false red.

## How this depends on the seeding work

A screenshot test needs the same input to produce the same pixels. A procedurally generated
maze does not, unless the generation seed is fixed — see
[REGRESSION_TESTING.md](./REGRESSION_TESTING.md). Before seeding existed, every page load
produced a different maze, so visual regression was impossible in principle, not just
unimplemented.

Seeding is now in place in the C# core (`MazeGenerationSettings.Seed`). What's missing is a
way to *reach* it from a URL.

## Prerequisite: URL-parameter seeding

The suite loads `/?seed=20260725&algorithm=backtracker&x=10&y=10&z=1` and expects that exact
maze. The web build must read those parameters at startup and apply them instead of using
menu defaults or randomised settings.

Sketch — Godot exposes the query string through `JavaScriptBridge`, which only exists on the
web export, so it must be feature-guarded:

```csharp
// Web-only: JavaScriptBridge is not available on desktop builds.
if (OS.HasFeature("web"))
{
    var search = JavaScriptBridge.Eval("window.location.search", true)?.ToString() ?? "";
    // Parse ?seed=&algorithm=&x=&y=&z= and apply onto MazeGenerationSettings,
    // then generate immediately, bypassing the menu.
}
```

Requirements for the parameters to be regression-safe:

1. **`seed` maps straight onto `MazeGenerationSettings.Seed`.** No re-randomising afterwards.
2. **Generation happens once, on load**, with no menu interaction needed — the test can't
   click through a UI reliably.
3. **Invalid or absent parameters fall back to current behaviour**, so normal visitors are
   unaffected.
4. **The render settles.** Whatever intro animation or camera easing exists must reach a
   fixed final frame; the harness waits for pixel stability and fails if it never settles.

Once that ships, set `MAZE_SEEDING=1` in the workflow to un-skip the suite and generate
baselines (below).

> Not implemented here because building the web export requires the patched Windows editor
> (see [WEB_EXPORT.md](./WEB_EXPORT.md)), so a Godot-side change could not be compiled or
> verified in this environment. Specified rather than guessed at.

## Layout

| Path | Purpose |
|---|---|
| `tests/visual/playwright.config.ts` | Projects, thresholds, fixed viewport/locale/timezone |
| `tests/visual/maze.spec.ts` | The real suite — screenshots the deployed build |
| `tests/visual/harness.spec.ts` | Self-test proving the harness works without a Godot build |
| `tests/visual/canvas-stability.ts` | Boot detection and pixel-stability polling |
| `tests/visual/*-snapshots/` | Committed baseline PNGs (platform-keyed by Playwright) |

## Why pixel-stability polling

A canvas app has no event meaning "finished drawing". Screenshotting on `domcontentloaded`
or after a fixed `waitForTimeout` captures a partially-drawn frame, and the resulting
baseline flakes forever afterwards.

`waitForStableFrame` instead downscales the canvas to 64×64, hashes the pixels, and waits
until consecutive samples agree. It **throws** if the canvas never settles — a still-animating
canvas is a reason to fail loudly, not to screenshot anyway and hope.

`waitForEngineBoot` additionally asserts `crossOriginIsolated` and `SharedArrayBuffer`, because
without COOP/COEP the canvas element appears but the .NET runtime never starts — and a
screenshot of that is a plausible-looking blank image.

## Thresholds, and the page-reuse trap

`maxDiffPixelRatio: 0.01` with per-pixel `threshold: 0.2`.

**Measured on this harness** (a seeded 2D-canvas fixture, Chromium 1194, one machine):

| Comparison | Result |
|---|---|
| Two screenshots, same page, no redraw | byte-identical |
| Two **fresh pages**, same seed | **byte-identical** |
| Two separate **browser launches**, same seed | **byte-identical** |
| Two `setContent` calls on **one reused page** | **3782 pixels differ (1.6%)** |

The headline: canvas rendering is deterministic, including across browser launches — but
**reusing a page across loads perturbs the raster**. The initial version of this suite reused
one page and looked like rendering nondeterminism; it was the fixture. Every test therefore
loads into a fresh page, and `canvas-stability.ts` callers must keep doing so.

The retained tolerance is for what *hasn't* been measured: different CI machines and driver
revisions, and the real build being a **WebGL/WASM** render rather than 2D canvas. If it
proves byte-stable in practice, tighten toward zero — and if a real regression ever slips
under 1%, tighten the ratio rather than adding per-test exceptions.

## Baselines

Playwright keys snapshots by platform (`...-linux.png`, `...-darwin.png`). CI runs Linux, so
**baselines must be generated on Linux** or CI fails on missing snapshots.

Generate/update them one of two ways:

```sh
# In CI (preferred): run the visual job with UPDATE_SNAPSHOTS=1 and commit the artifact.
# Locally on Linux:
cd tests/visual && npm ci && npx playwright test --update-snapshots
```

Do **not** generate baselines on macOS and commit them — they'll be named `-darwin` and CI
will still have nothing to compare against.

Updating a baseline is a reviewed act: the PR diff shows the old and new PNG side by side.
That's the whole value, so there's no auto-update-on-failure switch.

## CI wiring

Runs after the existing post-deploy smoke test, against the same deployed URL — the smoke
test proves it *boots*, this proves it *looks right*, and there's no point screenshotting a
build that didn't start.

Two jobs:

- **`visual-harness`** in `test.yml` — runs the self-test on every PR. No deploy needed, runs
  on Linux in seconds, and keeps the harness from rotting while the maze suite is skipped.
- **`visual-production` / `visual-preview`** in `web-export.yml` — runs the maze suite against
  the deployed URL, after the smoke test.

GitHub runners need the browser downloaded, exactly as the existing smoke job does it:

```yaml
- run: npm ci && npx playwright install --with-deps chromium
  working-directory: tests/visual
```

(The `CHROMIUM_PATH` env var in `playwright.config.ts` is only for constrained environments
that ship a pinned Chromium of a mismatched build — leave it unset in CI.)

On failure Playwright writes `expected`/`actual`/`diff` PNGs plus an HTML report — upload
`tests/visual/playwright-report/` and `test-results/` as artifacts, or a red build gives a
reviewer nothing to look at.

## Scope and limits

- **Covers** the deployed browser build's rendered output at fixed seeds and viewport.
- **Does not cover** interaction (solving, camera movement, menu flows) — that's Playwright
  functional testing, a separate suite if wanted.
- **Not a substitute for** the C# golden-file work: byte-comparing a serialized maze localises
  a regression to the algorithm, whereas a screenshot diff only says "the picture changed".
  Structure tests fail with a precise cause; visual tests catch what structure tests can't see.
- **Single browser.** Chromium only. Cross-browser rendering diffs are a different problem
  and would triple baseline count for little value on a WASM canvas.

# Build verification

How to tell **which build is being served** at [maze.ryankelly.dev](https://maze.ryankelly.dev),
and how that identity gets there.

Deploying is not the same as serving. Vercel uploads an artefact and then promotes an alias to
it; a failed or partial promotion leaves the previous build live while the job still reports
success. Everything here exists to close that gap.

## Three ways to check, cheapest first

| | How | Answers |
|---|---|---|
| **`build-info.json`** | `curl -s https://maze.ryankelly.dev/build-info.json` | What the deploy *claims*, without downloading ~96 MB of WASM |
| **About screen** | Menu → **About** | What the running application *believes it is* |
| **Smoke test** | Runs itself on every production deploy | Asserts both agree with the commit CI just built |

The first two can disagree, and that is the point. `build-info.json` is written next to
`index.html` by the deploy; the About screen reads constants **compiled into the WASM**. If a
deploy were partially replaced, or an old artefact re-promoted under a new sidecar, the two
would diverge — which neither one alone would reveal.

```console
$ curl -s https://maze.ryankelly.dev/build-info.json
{
  "commit": "12f29bd…",
  "branch": "main",
  "repository": "rtkelly13/ProceduralGeneration3DMazes",
  "runId": "30194540175",
  "runAttempt": "1",
  "builtUtc": "2026-07-26T09:15:03Z",
  "godotEditor": "4.7.1-stable"
}
```

Compare `commit` against the repository. `runId` links to the exact job:
`https://github.com/<repository>/actions/runs/<runId>`.

## Issuing a preview deploy without commenting `/preview`

`/preview` is an `issue_comment` trigger gated on `author_association == 'OWNER'`, so an
automated caller — an agent with GitHub API access but no ability to comment as the owner —
cannot use it. **Use `workflow_dispatch` instead.** It is not a workaround; for verifying a
branch it is strictly better:

| | `/preview` comment | `workflow_dispatch` |
|---|---|---|
| Needs an OWNER comment | yes | no |
| Workflow file used | **always the default branch** | **the ref you dispatch** |
| `.github/smoke/` used | default branch | the ref you dispatch |
| Deploy target | preview | `preview` (default), `production` only on `main` |

That second row is the one that matters. An `issue_comment` run cannot exercise a branch's own
changes to this workflow or to the smoke script — which is exactly how a build stamp added on a
branch arrived empty and a broken boot check went unnoticed.

```jsonc
// POST /repos/{owner}/{repo}/actions/workflows/web-export.yml/dispatches
{ "ref": "my-branch" }            // deploy_target defaults to "preview"
```

Then read the result from the run, cheapest call first:

1. **Step summaries** — the deploy step writes a `### preview deploy` table with the URL,
   commit, and a link to `build-info.json`; the smoke step writes a `### Served build` table and
   the bridge verdict. One short call, no log parsing.
2. **Job log markers** — `DEPLOY_URL=…` and `DEPLOY_TARGET=…` are printed on their own lines so
   a caller does not have to parse vercel's own output.

**Production is opt-in and `main`-only.** It used to be inferred — `refs/heads/main` meant
`--prod` — which put a dispatch on the wrong branch one step away from silently publishing to
`maze.ryankelly.dev`. The target is now stated explicitly, and `deploy_target=production` on any
other ref fails the job with a message rather than doing something surprising.

### If the deploy host is unreachable from where the agent runs

A sandboxed agent may be unable to reach `*.vercel.app` at all — this project's own environment
denies it at the proxy (`403` on `CONNECT`). **Do not try to route around that.** Dispatch the
workflow and read the verdict from the run instead: the smoke job runs inside CI, where the
deploy *is* reachable, and reports the served commit and the bridge state into the step summary.
That is the whole reason those checks are asserted in CI rather than left to a human to eyeball.

## The About screen

Menu → **About**, or `?test=1` and `window.__mazeCommand('{"cmd":"goto","scene":"about"}')`.

It shows the **full** 40-character commit rather than a short prefix — the screen exists so the
value can be compared against a workflow run, and an abbreviation weakens that for no benefit on
a screen with room. **Copy build details** puts the commit, branch, build time, run URL and
engine version on the clipboard in a form that can be pasted straight into an issue.

A build that CI did not stamp says so explicitly:

> **Provenance** — local build — not produced by CI, so it cannot be traced to a commit

That row is deliberate. The failure mode worth designing against is not a missing hash, it is a
**misleading** one: a locally built binary that looks official, or a stale value left over from
whenever a file was last written. `BuildInfo.IsOfficial` requires *both* a full 40-character hex
SHA *and* a CI run id, because only CI can supply the latter.

## How the stamp gets in

```
GitHub Actions
   └─ export-web action
        ├─ env: BuildCommit / BuildBranch / BuildRepository / BuildRunId / …
        │     └─ MSBuild reads env vars as properties
        │           └─ GenerateBuildStamp target  →  obj/BuildStamp.g.cs (constants)
        │                 └─ compiled into the WASM  →  CurrentBuild.Info  →  About screen
        │                                                                  →  test bridge state
        └─ writes build/web/build-info.json (sidecar, same inputs)
```

Three decisions are load-bearing.

**The action derives the stamp itself; caller inputs are only overrides.** A `/preview` run
proved why. For `issue_comment` events GitHub loads the **workflow file from the default
branch**, while a composite action is read from the *checked-out* PR head. So a PR that adds
`with: build_commit: …` has its own caller ignored: the action ran, `build-info.json` was
written, and every caller-supplied field arrived **empty**:

```json
{ "commit": "", "branch": "", "repository": "", "runId": "",
  "builtUtc": "2026-07-26T12:12:49Z", "godotEditor": "4.7.1-stable" }
```

Only the fields computed *inside* the action survived. The action now reads the commit from the
checkout (`git rev-parse HEAD`) and the repository and run from the `github` context, so the
stamp is correct regardless of which caller invoked it or whether it passed anything. The branch
is left blank when the checkout is detached — a wrong branch is worse than a missing one.

It also exposes a `build_commit` **output**. Assert against that rather than `github.sha`, which
is the default branch — not the PR head — on an `issue_comment`-triggered run.

**Compile-time constants, not a data file.** The obvious design — ship a `build-info.json` inside
the app and read it at startup — does not survive this project's web export.
`export_presets.cfg` uses `export_filter="all_resources"` with an empty `include_filter`, so a
plain `.json`/`.txt` file is **not packed into the PCK**. It would work on desktop and silently
report "unknown" in the browser, which is the one place the information is needed. Constants have
no packaging step to get wrong, no file I/O, and nothing for the trimmer to remove.

**Environment variables, not `-p:` arguments.** The export re-enters MSBuild *through the Godot
editor*, whose command line the workflow does not control. MSBuild reads environment variables as
properties, so that is the only channel reaching both the explicit `dotnet build` step and the
editor's own build. Both steps therefore set the same env block.

Unset locally by design: a developer build reports itself as a developer build.

## Verified

Each of these was executed, not reasoned about:

| Claim | Evidence |
|---|---|
| Env-var injection reaches the generated constants | `BuildCommit=… dotnet build` → the value appears in `BuildStamp.g.cs` |
| Unset properties produce a valid, compiling file | Local build with no env → empty constants, compiles |
| Repeat builds do not churn | `WriteOnlyWhenDifferent` — file mtime unchanged across two identical builds, so no forced recompile |
| The stamp reaches the running app | Building **stamped** made the scene test's "unstamped ⇒ Provenance row" assertion fail, and only that assertion — end to end through engine, autoload and UI |
| The About screen wires up | Headless Godot 4.7.1 scene test adds it to the tree so `_Ready` runs, then asserts the rows; **9/9 pass**, in both stamped and unstamped builds |
| `BuildInfo` logic | 22 NUnit tests, including that a short hash or a missing run id is *not* treated as official |

Not verified here: behaviour inside the **patched web export template**, which needs a real
deploy — the same standing gap as the test bridge (see
[TEST_BRIDGE.md](./TEST_BRIDGE.md#verification-status)).

## The smoke test's boot check was vacuous

Worth recording, because it invalidated a claim this project had been relying on. The check read:

```js
// waiting for engine to boot (canvas sized)…
() => { const c = document.querySelector("canvas"); return c && c.width > 0 && c.height > 0; }
```

An untouched `<canvas>` defaults to **300×150**, so `width > 0 && height > 0` is true before the
engine does anything. On a real preview it returned in **0.02 s** and reported
`canvas: {"w":300,"h":150}` followed by `✅ SMOKE PASS` — the default size, meaning Godot had not
resized it. Every "the deploy boots" result from this check was therefore unproven.

It now waits for the canvas to stop being the default size, which only the engine can cause, and
fails with the stuck dimensions and a pointer to the console output. Verified against a local
server serving both cases: a canvas that never resizes now fails, one that resizes passes, and a
`build-info.json` mismatch fails with both commits named. A sidecar that answers with HTML and
HTTP 200 — what an SPA catch-all does — warns instead of throwing a bare `SyntaxError`, which is
how it behaved when first written.

The lesson generalises: **a boot check must assert something only a booted app can produce.**
Presence of an element, or a dimension that is non-zero by default, is satisfied by the empty
page you are trying to rule out.

## For automation

The test bridge publishes the same identity, so a browser test can assert it:

```js
const state = JSON.parse(await page.evaluate(() => window.__mazeState));
expect(state.buildCommit).toBe(process.env.EXPECT_COMMIT);
```

Fields: `buildCommit`, `buildBranch`, `buildTime`, `buildRunId`. See
[TEST_BRIDGE.md](./TEST_BRIDGE.md).

The post-deploy smoke test asserts this automatically when `EXPECT_COMMIT` is set, which
`web-export.yml` supplies as `github.sha`. It fails the deploy on a mismatch — unlike the bridge
*presence* probe, which is only reported, because a wrong build being served is a genuine
user-facing defect rather than a missing convenience.

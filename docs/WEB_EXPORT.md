# Web Export (.NET / WebAssembly) — Experimental

This documents how the maze project is exported to the browser so it can be hosted
on a static site (e.g. Vercel). **This path is experimental** and relies on a
community-patched Godot build, because upstream Godot cannot yet export C#/.NET
projects to the web.

## Why this is non-trivial

Godot's official HTML5/WebAssembly export supports **GDScript only**. The .NET
(C#) web export is still unmerged upstream:

- Tracking issue: <https://github.com/godotengine/godot/issues/70796>
- Implementation PRs: [raulsntos #106125](https://github.com/godotengine/godot/pull/106125)
  (the .NET-web prototype), [static LibGodot #118976](https://github.com/godotengine/godot/pull/118976) (still open, "4.x")

Until that lands in an official release, exporting requires a **patched editor**:
[ComplexRobot/godot-dotnet-web-export](https://github.com/ComplexRobot/godot-dotnet-web-export)
— latest stable Godot with the raulsntos PR merged in. Its prebuilt binaries are
**Windows-only**, which shapes the build strategy below.

## Build strategy: GitHub Actions on a Windows runner

Because the patched editor only ships for Windows, the export runs in CI on a
`windows-latest` runner rather than locally on macOS. See
[`.github/workflows/web-export.yml`](../.github/workflows/web-export.yml).

Trigger it manually from the Actions tab (**workflow_dispatch**). Inputs:

| Input | Default | Meaning |
|-------|---------|---------|
| `godot_fork_tag` | `4.7-stable` | Release tag of the patched editor to download |
| `template_version` | `4.7.stable.mono` | Export-template folder name (must match the editor build) |

The job:

1. Installs .NET 9 SDK + the `wasm-tools` workload.
2. Downloads the patched editor zip from the fork release.
3. Installs the bundled `web_release.zip` / `web_debug.zip` templates in
   **self-contained mode** (next to the editor, via a `._sc_` marker — no AppData).
4. Registers the bundled local NuGet source if present.
5. `dotnet build -c ExportRelease /p:GodotTargetPlatform=web`.
6. Headless import + `godot --headless --export-release "Web" build/web/index.html`.
7. Uploads `build/web/**` (plus `export.log` / `import.log`) as the
   **`maze-web-export`** artifact.

Download the artifact to test the build, then host it (below).

> ✅ **Status: proven.** The project was migrated 4.5.1 → **4.7** to match the fork
> build, and the CI export job runs green: it produces a real C#→WASM build
> (~54 MB `index.wasm` with the .NET runtime + ~42 MB `index.pck`), confirmed by the
> wasm32 `.dotnet-publish-manifest` in `export.log`. Desktop build + full test suite
> (421 passing) remain green on Godot.NET.Sdk 4.7.0 / net9.0.

## Project-side changes already made

- **`ProceduralGeneration3DMazes.csproj`**
  - `TargetFramework` upgraded to `net9.0` (required by the .NET web runtime).
  - Web-only (`$(GodotTargetPlatform) == 'web'`): `OutputType=Exe`,
    `InvariantGlobalization=true`, and `TrimmerRootAssembly` roots for
    `System.Private.CoreLib` / `System.Runtime`.
  - `Program.cs` is excluded from non-web builds.
- **`Program.cs`** — minimal managed entry point required by the executable web build.
- **`export_presets.cfg`** — a `Web` preset with thread support and
  cross-origin-isolation headers enabled.
- **`.gitignore`** — ignores `build/` (export output).

## Local builds now need .NET 9

The project is on `net9.0`. Install the .NET 9 SDK locally (`dotnet --list-sdks`
should show a `9.0.x`) before `dotnet build` or opening in the Godot editor.
The desktop build, tests, and editor workflow are otherwise unchanged.

## Known limitations (from the patched runtime)

- **No GDExtension support** (the .NET runtime is built without position-independent code).
- **Invariant globalization** is forced — culture-specific formatting/parsing is unavailable.
- **Some BCL APIs do not work** (e.g. cryptographic APIs).
- Requires **SharedArrayBuffer** + cross-origin isolation in the browser (threads).

## Hosting on Vercel

The export is hosted as its **own static Vercel project** (not a path on the blog),
so its cross-origin-isolation headers can't affect any other site. The export uses
`SharedArrayBuffer`, which the browser only grants under cross-origin isolation, so
the project ships [`infra/vercel/vercel.json`](../infra/vercel/vercel.json) which the
CD step copies into the deploy root:

```json
{
  "headers": [
    { "source": "/(.*)", "headers": [
      { "key": "Cross-Origin-Opener-Policy", "value": "same-origin" },
      { "key": "Cross-Origin-Embedder-Policy", "value": "require-corp" }
    ]}
  ]
}
```

Without these headers the canvas loads but fails to start its worker threads.

### Deployment target (personal Vercel account)

| | |
|---|---|
| Vercel scope | `rtkelly13's projects` (personal / hobby), `team_359p3EN2vzqqcyrxavcNdc1x` |
| Project | `maze` — `prj_I4wwEWpZZ4lIMt5falO51Dezkkyq` |
| Custom domain | `maze.ryankelly.dev` (alias attached) |

Verified live (SSO/deployment protection disabled on this project only):
`200`, `application/wasm` for `index.wasm`, COOP/COEP headers present.

### CI/CD pipeline

| Trigger | Workflow | What runs |
|---|---|---|
| Any PR / push to `main` | `test.yml` | `dotnet test` (fast Linux gate — mark this the required check) |
| Push to `main` (or manual dispatch) | `web-export.yml` → `production` | Windows export → **production** deploy (`maze.ryankelly.dev`) |
| Comment `/preview` on a PR | `web-export.yml` → `preview` | Windows export → **preview** deploy; URL posted back as a comment |

The Windows export (the expensive step) is refactored into the composite action
`.github/actions/export-web` and only runs on merge to `main` or an explicit
`/preview` — never on every PR push. The patched-editor download is cached by fork
tag. `/preview` is restricted to OWNER/MEMBER/COLLABORATOR comment authors.

### Continuous deployment

The deploy steps publish the export to the `maze` project after a successful export.
They are **gated on three repo secrets** and skip cleanly until they're set:

```sh
# Use a SCOPED Vercel token (Vercel → Account Settings → Tokens), not the broad CLI token.
gh secret set VERCEL_TOKEN      --repo rtkelly13/ProceduralGeneration3DMazes
gh secret set VERCEL_ORG_ID     --repo rtkelly13/ProceduralGeneration3DMazes --body team_359p3EN2vzqqcyrxavcNdc1x
gh secret set VERCEL_PROJECT_ID --repo rtkelly13/ProceduralGeneration3DMazes --body prj_I4wwEWpZZ4lIMt5falO51Dezkkyq
```

### DNS for `maze.ryankelly.dev`

`ryankelly.dev` uses **external DNS** (not Vercel nameservers), so the subdomain
record must be added at the registrar — Vercel can't create it. Add:

```
maze   CNAME   cname.vercel-dns.com
```

Until then the alias is attached but won't resolve; the `*.vercel.app` URL works
immediately. SSL for the custom domain provisions automatically once DNS resolves.

### Post-deploy smoke test

Both the production and preview deploys are followed by a Playwright smoke test
(`.github/smoke/smoke.mjs`) that opens the *deployed* URL and asserts the build
actually **boots** — not merely that files are served:

- `crossOriginIsolated === true` and `SharedArrayBuffer` present (proves COOP/COEP
  are effective — the whole hosting risk),
- a `<canvas>` exists and the engine sizes it (boot signal),
- no fatal WASM/runtime `pageerror`.

A green deploy with a broken runtime therefore fails CI instead of silently shipping.

---

## Version matrix (keep these in lockstep)

Three things are version-locked; a mismatch causes a cryptic export failure. The
export action fails fast with a "version drift" error if the first two disagree.

| Piece | Where | Current |
|-------|-------|---------|
| `Godot.NET.Sdk` | `ProceduralGeneration3DMazes.csproj` | `4.7.0` |
| Export-template version | workflow `template_version` input | `4.7.stable.mono` |
| Patched-editor release tag | workflow `godot_fork_tag` input | `4.7-stable` |
| `.NET` SDK | `global.json` | `9.0.315` |
| Editor binary SHA-256 | `.github/editor-checksums.txt` | pinned |

When you bump Godot, **all four rows move together.**

### Updating the pinned editor (fork bump)

1. Pick the new fork release tag (must match a `Godot.NET.Sdk` version you can build with).
2. Download its `Godot_v<tag>_mono_web_export_win64.zip` and record the hash:
   ```sh
   shasum -a 256 Godot_v<tag>_mono_web_export_win64.zip
   ```
   Add/replace the line in `.github/editor-checksums.txt`.
3. Bump `Godot.NET.Sdk/<x.y.z>` in the `.csproj`, `template_version`, and
   `godot_fork_tag` defaults; open the project once in the matching editor to
   migrate `project.godot`.
4. Open a PR and `/preview` it — the smoke test confirms the new toolchain boots.

---

## Maintenance & what to watch going forward

The web export rests on **experimental, third-party** foundations. Treat it as
something to monitor, not set-and-forget.

- **Official .NET web export is the finish line.** Track godotengine/godot
  [#118976](https://github.com/godotengine/godot/pull/118976) (static LibGodot) and
  the prototype [#106125](https://github.com/godotengine/godot/pull/106125). **When
  official web export ships, migrate off the fork** — repoint the composite action's
  `fork_repo` (and drop the custom editor entirely if upstream templates suffice).
- **Single-maintainer fork risk.** The editor comes from one community fork
  (`ComplexRobot/godot-dotnet-web-export`). If it goes stale (no build for a Godot
  version you need), you're stuck. Mitigations in place: the `fork_repo`/tag are
  **inputs** (easy to repoint at a mirror), and the binary is **checksum-pinned**.
  Recommended: **mirror the exact editor zip you rely on** to your own release/storage
  so a deleted upstream release can't break CI.
- **Runtime feature gaps can bite silently.** The patched runtime has **no
  GDExtension**, **forced invariant globalization**, and **missing crypto BCL APIs**.
  Code using culture-specific formatting/parsing or `System.Security.Cryptography`
  will work on desktop and **fail only on web**. The smoke test catches a dead boot,
  not subtle feature divergence — if you add such code, test it on web explicitly.
- **Payload & caching.** The build is ~96 MB (54 MB `index.wasm` + 42 MB
  `index.pck`). Vercel already serves it **Brotli-compressed**, and assets use
  `must-revalidate` (ETag → 304), so there's no staleness. The remaining win —
  `immutable` long-cache — needs **content-hashed filenames**, which the Godot export
  uses fixed names for (`index.wasm`). That's the future improvement if load time
  matters; don't set `immutable` on the fixed-name files or redeploys serve stale bytes.
- **`/preview` trust boundary.** The command is restricted to **OWNER** comments and
  checks out the PR head before running the composite action with secrets in scope.
  Keep it owner-only; if you ever add trusted collaborators, consider running the
  action from the base ref instead of PR head.
- **Windows runner minutes.** The export runs on `windows-latest` (billed at a
  premium) on every merge to `main` and every `/preview`. Editor download is cached
  by fork tag. If minutes get tight, gate the production export to release tags only.
- **Vercel token hygiene.** CI uses a scoped `VERCEL_TOKEN`. Rotate it periodically;
  if it expires, deploys fail at the deploy step (export still succeeds) — re-run
  `gh secret set VERCEL_TOKEN`.

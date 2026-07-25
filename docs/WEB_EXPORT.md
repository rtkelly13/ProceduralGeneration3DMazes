# Web Export (.NET / WebAssembly) — Experimental

This documents how the maze project is exported to the browser so it can be hosted
on a static site (e.g. Vercel). **This path is experimental** and relies on a
community-patched Godot build, because upstream Godot cannot yet export C#/.NET
projects to the web.

> 📍 **This document = how it works today.** For the plan to get onto officially
> supported foundations — upstream status, exit criteria, staged migration, monitoring
> cadence and rollback — see **[WEB_EXPORT_ROADMAP.md](./WEB_EXPORT_ROADMAP.md)**.

## Why this is non-trivial

Godot's official HTML5/WebAssembly export supports **GDScript only**. The .NET
(C#) web export is still unmerged upstream — .NET's WASM build expects to be the main
module and doesn't support dynamic linking, so Godot can't load it.

Status as of 2026-07-25 (see the [roadmap](./WEB_EXPORT_ROADMAP.md#1-where-things-actually-stand)
for the full picture and the dependency chain):

- Tracking issue [#70796](https://github.com/godotengine/godot/issues/70796) — still open.
- [#118976](https://github.com/godotengine/godot/pull/118976) (static LibGodot .NET web
  export) — draft, milestone `4.x`. **This is the PR that ends the hack.**
- [#121502](https://github.com/godotengine/godot/pull/121502) — LibGodot on web, opened
  2026-07-17. Core-level prerequisite, not C#-specific.
- [#106125](https://github.com/godotengine/godot/pull/106125) — the prototype this
  fork is built from. Draft, stalled on .NET 10, effectively superseded.

Until it lands in an official release, exporting requires a **patched editor**:
[ComplexRobot/godot-dotnet-web-export](https://github.com/ComplexRobot/godot-dotnet-web-export)
— latest stable Godot with the raulsntos PR merged in. Its prebuilt binaries are
**Windows-only**, which shapes the build strategy below.

## Build strategy: GitHub Actions on a Windows runner

Because the patched editor only ships for Windows, the export runs in CI on a
`windows-latest` runner rather than locally on macOS. See
[`.github/workflows/web-export.yml`](../.github/workflows/web-export.yml).

Trigger it manually from the Actions tab (**workflow_dispatch**). Both inputs are
**blank by default** and resolve from [`.github/web-toolchain.env`](../.github/web-toolchain.env);
fill one in only to test an unpinned editor build ad hoc.

| Input | Default | Meaning |
|-------|---------|---------|
| `godot_fork_tag` | *(blank → toolchain file)* | Release tag of the patched editor to download |
| `template_version` | *(blank → toolchain file)* | Export-template folder name (must match the editor build) |

The job:

1. Resolves the toolchain versions and **fails fast** on version drift or a missing
   checksum pin — before downloading a 165 MB editor.
2. Installs .NET 9 SDK + the `wasm-tools` workload.
3. Downloads the patched editor zip from the fork release.
4. Verifies the zip against its **pinned SHA-256**, matching the expected filename
   exactly (a restored cache can't substitute a different editor).
5. Installs the bundled `web_release.zip` / `web_debug.zip` templates in
   **self-contained mode** (next to the editor, via a `._sc_` marker — no AppData).
6. Registers the bundled local NuGet source if present.
7. `dotnet build -c ExportRelease /p:GodotTargetPlatform=web`.
8. Headless import + `godot --headless --export-release "Web" build/web/index.html`.
9. Uploads `build/web/**` (plus `export.log` / `import.log`) as the
   **`maze-web-export`** artifact.

Download the artifact to test the build, then host it (below).

> ✅ **Status: proven.** The project was migrated 4.5.1 → **4.7** to match the fork
> build, and the CI export job runs green: it produces a real C#→WASM build
> (~54 MB `index.wasm` with the .NET runtime + ~42 MB `index.pck`), confirmed by the
> wasm32 `.dotnet-publish-manifest` in `export.log`. Desktop build + full test suite
> (421 passing) remain green on net9.0.
>
> Now pinned to **4.7.1** (upstream 4.7.1 is a 78-fix stability release with no known
> incompatibilities with 4.7). Verify with a `/preview` run before relying on it.

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

✅ **Done — resolving as of 2026-07-25.** `maze.ryankelly.dev` is a CNAME to
`cname.vercel-dns.com`, which resolves to Vercel's anycast addresses
(`76.76.21.x` / `66.33.60.x`). SSL provisions automatically once DNS resolves, so
nothing further is needed here.

Recorded for future reference: `ryankelly.dev` uses **external DNS** (not Vercel
nameservers), so subdomain records must be added at the registrar — Vercel can't
create them. The record is:

```
maze   CNAME   cname.vercel-dns.com
```

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

Several pieces are version-locked; a mismatch causes a cryptic export failure. The
editor tag and template version live in **one** file —
[`.github/web-toolchain.env`](../.github/web-toolchain.env) — and every workflow reads
them from there. **Never** duplicate them into workflow or action defaults: that's how
`production` and `/preview` end up building different toolchains.

| Piece | Where | Current |
|-------|-------|---------|
| Patched-editor release tag | `.github/web-toolchain.env` → `GODOT_FORK_TAG` | `4.7.1-stable` |
| Export-template version | `.github/web-toolchain.env` → `GODOT_TEMPLATE_VERSION` | `4.7.1.stable.mono` |
| Fork repo (or mirror) | `.github/web-toolchain.env` → `GODOT_FORK_REPO` | `ComplexRobot/godot-dotnet-web-export` |
| `Godot.NET.Sdk` | `ProceduralGeneration3DMazes.csproj` | `4.7.1` |
| `.NET` SDK | `global.json` | `9.0.315` |
| Editor binary SHA-256 | `.github/editor-checksums.txt` | pinned |

When you bump Godot, **the first four rows move together.** The export action's guard
enforces this at **patch level** — it normalises the three spellings of the same
version (`4.7.1`, `4.7.1.stable.mono`, `4.7.1-stable`) and requires exact agreement,
then checks a checksum is pinned for the target asset. All of that runs *before* the
165 MB editor download, so a mistake costs seconds.

> Godot's naming convention: `x.y` releases have **no** patch component in the template
> name (`4.7.stable.mono`) while `x.y.z` releases do (`4.7.1.stable.mono`). The guard
> handles both — a csproj SDK of `4.7.0` is treated as `4.7`.

### Updating the pinned editor (fork bump)

1. Pick the new fork release tag — a **stable** one
   ([why](./WEB_EXPORT_ROADMAP.md#phase-1--track-upstream-releases-promptly-routine)) —
   and confirm a matching `Godot.NET.Sdk` exists on NuGet.
2. Record the editor hash and **add** (don't replace) the line in
   `.github/editor-checksums.txt` — keeping old entries makes rollback a one-line
   revert:
   ```sh
   TAG=4.7.1-stable
   curl -sL "https://github.com/ComplexRobot/godot-dotnet-web-export/releases/download/$TAG/Godot_v${TAG}_mono_web_export_win64.zip" \
     | shasum -a 256
   ```
   (Piping avoids keeping a 165 MB file around; drop the pipe if you want the zip.)
3. Update `GODOT_FORK_TAG` and `GODOT_TEMPLATE_VERSION` in
   `.github/web-toolchain.env`, and `Godot.NET.Sdk/<x.y.z>` in the `.csproj`. Open the
   project once in the matching editor to migrate `project.godot`.
4. Open a PR and `/preview` it — the smoke test confirms the new toolchain boots.
   The drift guard will reject the PR if you missed one of the three version rows.

---

## Maintenance & what to watch going forward

The web export rests on **experimental, third-party** foundations. Treat it as
something to monitor, not set-and-forget.

- **Official .NET web export is the finish line.** The full status, dependency chain,
  exit criteria and staged migration plan now live in
  **[WEB_EXPORT_ROADMAP.md](./WEB_EXPORT_ROADMAP.md)** — that's the document to read and
  keep updated. Short version: LibGodot Core
  [#110863](https://github.com/godotengine/godot/pull/110863) **merged and shipped in
  4.6**, with [#121502](https://github.com/godotengine/godot/pull/121502) (LibGodot on
  web) and [#118976](https://github.com/godotengine/godot/pull/118976) (.NET on top)
  still open at milestone `4.x`. Direction is good; no date.
- **Single-maintainer fork risk — lower than first assumed.** The editor comes from one
  community fork (`ComplexRobot/godot-dotnet-web-export`), but it tracked upstream
  4.7.1 (released 2026-07-14) with a matching build on **2026-07-16** and also ships
  4.8 dev snapshots — actively maintained, not stale. Mitigations in place:
  `GODOT_FORK_REPO`/tag are **config** (one-line repoint at a mirror), and the binary
  is **checksum-pinned**. Still outstanding: **mirror the exact editor zip you rely on**
  to storage you control, so a deleted upstream release can't break CI — tracked as
  [Phase 0](./WEB_EXPORT_ROADMAP.md#phase-0--reduce-hack-surface-actionable-now-no-upstream-dependency).
- **Runtime feature gaps can bite silently.** The patched runtime has **no
  GDExtension**, **forced invariant globalization**, and **missing crypto BCL APIs**.
  Code using culture-specific formatting/parsing or `System.Security.Cryptography`
  will work on desktop and **fail only on web**. The smoke test catches a dead boot,
  not subtle feature divergence — if you add such code, test it on web explicitly via
  `/preview`. A static guard to catch this at PR time is
  [Phase 0](./WEB_EXPORT_ROADMAP.md#phase-0--reduce-hack-surface-actionable-now-no-upstream-dependency)
  work and **not yet implemented**.
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
  by **fork repo + tag** (so repointing at a mirror correctly misses the cache rather
  than reusing the old binary). If minutes get tight, gate the production export to
  release tags only. Phase 3 of the roadmap removes the Windows runner entirely.
- **Vercel token hygiene.** CI uses a scoped `VERCEL_TOKEN`. Rotate it periodically;
  if it expires, deploys fail at the deploy step (export still succeeds) — re-run
  `gh secret set VERCEL_TOKEN`.

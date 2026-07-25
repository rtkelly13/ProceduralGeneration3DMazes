# Web Export: Road to Stable

**Purpose.** [`WEB_EXPORT.md`](./WEB_EXPORT.md) documents how the experimental C#→WASM
export works *today*. This document is the plan for getting it onto **officially
supported** foundations — what we're waiting for, how we'll know it's ready, what we
migrate in what order, and what we do in the meantime.

**Last reviewed:** 2026-07-25 · **Next review due:** 2026-10-25 (quarterly — see
[Monitoring](#monitoring))

---

## 1. Where things actually stand

Official Godot **still cannot** export C#/.NET projects to the web. The hosting hack
remains necessary. But the upstream approach changed materially in the last year, and
the change is favourable.

### The root problem (unchanged)

.NET's WASM build expects to **be the main module** and does not support dynamic
linking, so Godot cannot load it as a library. Everything below is a strategy for
getting around that single fact.

### Upstream state as of 2026-07-25

| Item | State | Note |
|---|---|---|
| [#70796](https://github.com/godotengine/godot/issues/70796) — tracking issue | **Open** since Jan 2023 | No milestone. The canonical "is it done yet" link. |
| [#106125](https://github.com/godotengine/godot/pull/106125) — `[.NET] Add web export support` (raulsntos) | **Draft** | The original prototype, and the ancestor of the fork we use. Stalled on .NET 10 compatibility. Effectively superseded. |
| [#110863](https://github.com/godotengine/godot/pull/110863) — `LibGodot: Core` | ✅ **Merged 2025-10-08 — shipped in Godot 4.6** | Builds the engine as a library. The foundation of the new approach. |
| [#118976](https://github.com/godotengine/godot/pull/118976) — `[.NET] web export using static LibGodot` | **Draft**, milestone `4.x` | The .NET-specific consumer of LibGodot. This is the one that ends the hack. |
| [#121502](https://github.com/godotengine/godot/pull/121502) — `Extend LibGodot Core to the web platform` | **Open** (2026-07-17), milestone `4.x` | Brings LibGodot to web. **Core-level, not C#-specific** — its description does not mention .NET. |

### What changed, and why it matters

The strategy pivoted from *"patch the .NET runtime into the engine's WASM"* (#106125 —
fragile, stalled) to *"make Godot a library so .NET **can** be the main module"*
(LibGodot). That reframing dissolves the root problem instead of working around it,
and **its first half is already in a shipped release (4.6)**.

Read the dependency chain as:

```
LibGodot: Core (#110863)  ──✅ merged, in 4.6
        └─> LibGodot on web (#121502)  ──open, milestone 4.x
                └─> .NET web export on static LibGodot (#118976)  ──draft, milestone 4.x
                        └─> official C# web export  ──not scheduled
```

**Honest read on timing:** two open, unscheduled PRs still sit between us and official
support, and milestone `4.x` means "no committed release". Plan for **4.9 at the
earliest**, and do not plan work around it landing. The direction is now good; the
date is not knowable.

---

## 2. Definition of done

We declare the web export **stable** and retire the hack when *all* of these hold:

1. A **stable** (not dev/beta/RC) official Godot release can export a C#/.NET project
   to web using **official export templates** — no patched editor.
2. The official path runs on **Linux CI**, letting us drop the `windows-latest` runner.
3. Our project exports and **boots** on it — verified by the existing
   [`smoke.mjs`](../.github/smoke/smoke.mjs) test, unchanged.
4. The runtime limitations we currently accept are either resolved or still acceptable:
   invariant globalization, no GDExtension, missing crypto BCL APIs.
5. `Godot.NET.Sdk` and the export templates come from the same official release, so
   the version-drift guard collapses to a single upstream version.

Criteria 1–3 are the hard gates. 4–5 are cleanup that follows.

---

## 3. Migration plan

Staged so each phase is independently valuable and independently revertible. Phases 0
and 1 need no upstream progress; 2 onward are trigger-driven.

### Phase 0 — Reduce hack surface *(actionable now, no upstream dependency)*

Make the current setup cheap to maintain and cheap to leave.

- [x] **Single source of truth for toolchain versions** — [`.github/web-toolchain.env`](../.github/web-toolchain.env).
      Previously the tag/template were duplicated across the composite action's
      defaults, the `workflow_dispatch` defaults and inline fallbacks in the
      `production` job, so `production` and `/preview` could silently build different
      toolchains.
- [x] **Patch-level drift guard.** The guard compared only `major.minor`, so a
      4.7.0-vs-4.7.1 mismatch passed. It now normalises all three spellings
      (`4.7.1` / `4.7.1.stable.mono` / `4.7.1-stable`) and requires exact agreement,
      and fails *before* the 165 MB editor download if the target tag has no pinned
      checksum.
- [ ] **Mirror the editor binary.** The single highest-value remaining item. Copy the
      exact editor zip we depend on to storage we control (a release asset on this
      repo, or S3), then point `GODOT_FORK_REPO` at it. Today a deleted upstream
      release breaks CI outright. The checksum pin means a mirror is trivially
      verifiable — it must hash to the value already in
      [`editor-checksums.txt`](../.github/editor-checksums.txt).
- [ ] **Guard the feature gaps in code, not prose.** Invariant globalization and the
      missing crypto APIs fail *only on web* — desktop builds and the test suite stay
      green. Add an analyzer/test that rejects culture-sensitive formatting and
      `System.Security.Cryptography` usage in `scripts/`, so the failure surfaces at
      PR time rather than on a deploy.

### Phase 1 — Track upstream releases promptly *(routine)*

Stay on the current *stable* Godot line. The fork has proven fast (§4), so this is
low-cost, and staying current means the eventual official switch is a small diff
rather than a multi-version jump.

- Follow **stable** fork releases only. **Skip `dev`/`beta`/`rc` builds** — there's no
  upside for a hosted demo.
- Procedure: [`WEB_EXPORT.md` → "Updating the pinned editor"](./WEB_EXPORT.md#updating-the-pinned-editor).

### Phase 2 — Evaluate on the first official preview

**Trigger:** #118976 (or a successor) merges, *or* an official dev/beta snapshot ships
C# web export templates.

- Branch, point `GODOT_FORK_REPO`/`GODOT_FORK_TAG` at the official build, `/preview`,
  and let the smoke test judge it. **Do not touch production.**
- Record what breaks. Expect friction around `OutputType=Exe`, `Program.cs`,
  `InvariantGlobalization` and the `TrimmerRootAssembly` roots — the official path may
  need none of them, some of them, or different ones.
- Outcome: a written go/no-go against §2, not a deploy.

### Phase 3 — Switch production

**Trigger:** §2 criteria 1–3 all satisfied on a stable release.

1. Bump `Godot.NET.Sdk`, the .NET SDK in `global.json` if required, and the toolchain
   file to the official version.
2. Move the export job from `windows-latest` to `ubuntu-latest`.
3. Delete the fork-specific machinery from the composite action: editor download,
   checksum verification, self-contained `._sc_` template install, bundled NuGet
   source registration. Replace with the standard `godot --headless --export-release`
   against official templates.
4. **Keep unchanged:** the Vercel project, `vercel.json` COOP/COEP headers, and
   `smoke.mjs`. Cross-origin isolation is a *hosting* requirement of threaded WASM,
   not an artefact of the fork — it will still be needed.
5. Retire `.github/editor-checksums.txt` and the fork rows of the version matrix.

### Phase 4 — Clean up project-side workarounds

**Trigger:** production green on the official path for two weeks.

- Re-test whether `Program.cs`, `OutputType=Exe` and `InvariantGlobalization=true` are
  still required; drop each that isn't, and with it the conditional-compilation
  complexity in the `.csproj`.
- Revisit `immutable` long-cache headers if official export produces content-hashed
  filenames (see [`WEB_EXPORT.md`](./WEB_EXPORT.md) → payload & caching).
- Fold this document's remaining content back into `WEB_EXPORT.md` and delete it.

---

## 4. Risk register

| Risk | Current assessment | Mitigation |
|---|---|---|
| **Fork goes stale** | **Lower than previously assumed.** The fork tracked upstream 4.7.1 (released 2026-07-14) with a matching build on **2026-07-16** — two days — and also ships 4.8 dev snapshots. It is actively maintained, not abandoned. | `GODOT_FORK_REPO` is an input, so repointing is a one-line change. **Still do the mirror** (Phase 0) — maintained today ≠ available forever. |
| **Upstream release deleted/republished** | Breaks CI immediately; checksum mismatch would also fail the build. | Checksum pin catches substitution. Mirror (Phase 0) removes the availability risk. |
| **Silent runtime feature divergence** | Real and unaddressed. Culture-sensitive formatting or crypto passes desktop tests and fails only in the browser; the smoke test catches a dead boot, not subtle breakage. | Phase 0 static guard. Until then, test on web explicitly via `/preview` when touching formatting or crypto. |
| **Both upstream PRs stall** | Plausible — #106125 already did. | Phase 0/1 keep the hack maintainable indefinitely. Nothing here has a deadline. |
| **Windows runner cost** | Windows minutes bill at a premium; runs on every merge to `main` and every `/preview`. | Editor download is cached by repo+tag. If minutes get tight, gate production export to release tags. Phase 3 removes this entirely. |
| **`/preview` trust boundary** | Owner-only, but checks out PR head with secrets in scope. | Keep it `OWNER`-only. If collaborators are ever added, run the action from the base ref instead. |

---

## 5. Monitoring

Quarterly is the right cadence — this moves on a scale of engine releases, not weeks.
Checking more often is wasted effort; not checking means missing the switch.

**Each review (next due 2026-10-25):**

1. Check the four upstream links in §1 for state changes. #121502 → #118976 is the
   chain that matters; #70796 closing is the unambiguous signal.
2. Check the [fork's releases](https://github.com/ComplexRobot/godot-dotnet-web-export/releases)
   for a newer **stable** tag; if there is one, do a Phase 1 bump.
3. Confirm the live demo still boots (the smoke test covers this on every deploy, but
   confirm a deploy has actually run since the last review).
4. Update §1's table, the "Last reviewed" date, and this file's next-review date.

Also worth a look at each **new Godot minor release's** release notes — official C#
web support would be headline news there, and 4.6 shipping LibGodot Core is exactly
the kind of change that showed up that way.

---

## 6. Rollback

If a toolchain bump breaks the deploy, revert in this order — each step is independent:

1. **Revert the pin.** Set `GODOT_FORK_TAG` / `GODOT_TEMPLATE_VERSION` in
   [`web-toolchain.env`](../.github/web-toolchain.env) back to the previous values and
   `Godot.NET.Sdk` in the `.csproj` to match. Superseded checksums are kept in
   `editor-checksums.txt` deliberately, so a rollback needs no checksum work.
2. **Re-deploy.** Push to `main`, or promote the last-known-good deployment in the
   Vercel dashboard for an immediate fix without waiting on a Windows export.
3. The drift guard will reject a partial revert — if it fires, one of the three
   version rows was missed.

# Automated Regression Testing

**Status:** Foundation landed (seeding + determinism guards). Golden-file suite designed
below, **not yet built**.

> This document covers the *generation* regression story. For which testing layer to use for
> what — unit, in-engine scene, browser functional, visual — see **[TESTING.md](./TESTING.md)**.

## Why the existing suite can't catch regressions

The 421 tests before this work were all *invariant* tests: is the maze valid, is every cell
reachable, is the path bidirectional. Valuable, but they share a blind spot — none of them
can assert **what** was generated, only that whatever came out satisfies some property.

That was not a gap in test-writing discipline. It was forced: generation was
nondeterministic, so there was nothing stable to assert against. Measured on the
pre-seeding code (12×12×2, identical settings, 8 runs each):

| Algorithm | Distinct mazes / 8 runs |
|---|---|
| GrowingTree | 8/8 |
| RecursiveBacktracker | 8/8 |
| BinaryTree | 8/8 |
| Prims | 8/8 |

Distinct start points: 8/8. Distinct end points: 8/8. Shortest-path length across eight
identical-setting runs: `[45, 1, 33, 25, 25, 104, 43, 151]` — **min 1, max 151**.

Consequences:

- **No output assertions.** An algorithm change that made mazes measurably worse — more
  dead ends, shorter solution paths, biased carving — would pass every test.
- **Runtime was unpredictable.** Unlucky shuffle orders send `PerfectAgent`'s exponential
  DFS down enormous subtrees, so suite wall-time on identical code ranged from **~1s to a
  40-minute hang** (CI run 9), with **1002s on `main`** well before any of this work. That
  reads as flaky infrastructure; it was unseeded input meeting an exponential algorithm.
  Note this bit the *sample-maze* tests, which load mazes from disk — so the nondeterminism
  there is the agent's own shuffle order, not generation. See "Open questions".
- **Bugs weren't reportable.** A failure found by chance could not be reproduced, because
  nothing recorded the random state that produced it.

That `ShortestPath == 1` case is worth its own look: it means start and end landed adjacent,
producing a trivially solvable maze. Whether that's acceptable is a product question, but it
is currently *unobservable* — exactly what regression tests should surface.

## What landed: the seeding foundation

Three independent sources of ungoverned randomness fed generation, none of them seedable:

1. `RandomValueGenerator` → a `[ThreadStatic]` `Random` seeded from `Environment.TickCount`.
2. `ArrayHelper.Shuffle` → `Random.Shared`, a process-global. Called from the backtracker,
   the growing-tree algorithm and the random carver — and reached as a *static*, so there
   was no injection seam at all.
3. `RandomAgent` and `PerfectAgent` → `Random.Shared.Shuffle` directly, bypassing even
   `ArrayHelper`.

Now:

- **`IRandomValueGenerator` is the single source of randomness**, injected everywhere, with
  `GetNext`, `Shuffle` and `Reseed`. Instances hold their own `Random` — no thread-static, no
  shared global. (The thread-static indirection was also unnecessary: nothing in the maze
  pipeline is concurrent.)
- **`MazeGenerationSettings.Seed`** (`int?`) pins a run. `MazeGenerationFactory.GenerateMaze`
  reseeds once, before anything consumes randomness, so the seed determines start/end
  placement, carving order, wall removal and agent walks alike.
- **`MazeGenerationResults.Seed`** always reports the seed used. An unseeded run still draws
  a concrete seed and reports it, so a maze found by chance can be reproduced — feed the
  reported seed back in.
- **`ArrayHelper` is gone.** Its shuffle overloads were the trap; its `Average` was dead
  code. Benchmarks now measure the production shuffle path with a fixed seed.
- **`ISystemClock`** replaces a `DateTime.Now` read inside `MazeStatsSerializer`, which
  would otherwise have made serialized stats differ on every run — a golden-file blocker
  found by the discipline guard below, not by inspection.

### Guards

- **`DeterminismTests`** — same seed produces byte-identical mazes across all four
  algorithms; different seeds still produce different mazes (so seeding can't silently
  collapse the output space); an unseeded run's reported seed reproduces its maze; wall
  removal and agent walks are seed-stable; and reusing one container across seeds doesn't
  leak state between runs.
- **`RandomnessDisciplineTests`** — scans `scripts/maze/` and fails on `Random.Shared`,
  `new Random(`, `ArrayHelper.Shuffle`, `Guid.NewGuid` and `DateTime.Now/UtcNow`, with
  `RandomValueGenerator.cs` and `SystemClock.cs` as the only exemptions. Determinism is a
  whole-pipeline property: one stray global call breaks it, and the symptom appears as a
  flaky golden test far from the cause. This catches it at the source.

## The regression suite (designed, not built)

### Layer 1 — Golden files

Commit the serialized output of a fixed matrix of seeded generations; the test regenerates
and byte-compares.

- **Matrix:** 4 algorithms × 3 sizes (small 2D, medium 2D, small 3D) × 2 seeds ≈ 24 cases.
  Enough to cover each algorithm and dimensionality without a large fixture set.
- **Stored under** `tests/golden/<algorithm>-<size>-<seed>.maze`, using the existing
  `.maze` format so goldens stay human-diffable and the serializer gets exercised too.
- **On failure:** print the seed and a unified diff, plus the command to regenerate.
- **Regeneration:** one opt-in switch (`UPDATE_GOLDENS=1`) that rewrites the fixtures.
  Deliberately env-gated, because the whole value of a golden file is that updating it is a
  reviewed act — a diff in the PR, not a silent overwrite.

**The judgement call this layer forces:** a golden file fails on *any* output change,
including a deliberate improvement. That's the point — it makes intent explicit — but it
means algorithm work will routinely carry golden churn. If that proves annoying in practice,
the answer is to narrow what's goldened (structure only, not stats), not to loosen the
comparison.

### Layer 2 — Metric assertions

Golden files detect *change*; they don't say whether it's good. Layer 2 asserts on quality
metrics from `GenerationMetrics` / `MazeStatsResult` — dead-end count, junction count,
branching factor, solution-path length — as **ranges** over a sample of seeds.

Ranges, not exact values, because these should express algorithm character ("a backtracker
maze has long corridors and few junctions") and survive an unrelated refactor. This is the
layer that catches "still valid, but measurably worse".

### Layer 3 — Cross-seed invariants (property tests)

Run the existing invariant checks across many seeds rather than one arbitrary one — a
lightweight property test. Cheap to add now that a failing case is reportable by seed, and
it's how the `ShortestPath == 1` class of finding gets caught systematically.

### Layer 4 — Performance regressions

BenchmarkDotNet already exists but isn't wired to CI. With seeded input its numbers become
comparable run to run, which is the prerequisite. Deferring: benchmark-in-CI needs a
stable-hardware story before threshold failures mean anything, and GitHub runners are noisy.

### CI wiring

Layers 1–3 are plain NUnit tests, so `test.yml` picks them up with no workflow change —
which is the main argument for building them in that order. Layer 4 needs its own job and
should wait.

## Recommended order

1. **Layer 1** on the 4 algorithms at one size — proves the harness, small fixture set.
2. **Layer 3** — cheapest real bug-finding per line of test code.
3. **Layer 2** — needs a baseline sample of what current metrics actually are, so it comes
   after there's a stable way to generate them.
4. **Layer 4** — only with a considered hardware/threshold story.

## Open questions

- **Golden scope:** maze structure only, or stats JSON too? Structure alone is more stable;
  stats catch more. Recommendation: structure first, add stats if a real regression escapes.
- **Should `Seed` be surfaced in the UI?** The plumbing now supports "regenerate this exact
  maze" and "share a seed". That's a product feature, not a testing need — worth a separate
  decision.
- **Should the `.maze` format carry its seed?** An optional `SEED` header would make an
  exported maze self-describing. It's a format change, so it needs a spec update in
  `SerialisationSpecification.md` and a deserializer that tolerates the field's absence.
- **Is `ShortestPath == 1` acceptable?** Reachable today via random start/end placement. If
  not, minimum-separation becomes a generation constraint and a Layer 3 assertion.
- **`BinaryTreeAlgorithm` is a placeholder** that delegates to `BacktrackerAlgorithm`
  (see its own comment). Golden files would lock in that duplicate behaviour — worth
  resolving before, not after, goldens are committed.
- ~~**`PerfectAgent`'s search is worst-case exponential**~~ — **fixed.** It now uses a shared
  visited set and an explicit stack instead of scanning the current path
  (`previousPoints.Any(...)`) and copying the whole path per branch.

  Before: 8 runs of the two sample-maze tests gave 1.8s, 1.9s, 2.0s, 3.2s, 14.6s, 25.9s and
  **two runs unfinished at 120s**. After: 1.8–2.8s, zero timeouts. Full suite dropped from 8s
  to 1s. The iterative form also removes a stack-overflow ceiling that scaled with path
  length — the app allows 50 cells per axis, i.e. tens of thousands of cells.

  The temporary workaround (narrowing the PerfectAgent sample to 200 cells) has been removed,
  restoring the 4 dropped test cases. The 60s per-test cap and `timeout-minutes: 15` on the
  CI job stay as tripwires: cheap, and a regression previously cost a 40-minute job.

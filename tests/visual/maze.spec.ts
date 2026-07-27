import { test, expect } from "@playwright/test";
import { waitForEngineBoot, waitForStableFrame, failOnRuntimeErrors } from "./canvas-stability";

/**
 * Visual regression against the deployed C#/WASM build.
 *
 * Requires MAZE_URL (a deployed preview or production URL) — the build cannot be produced
 * locally on Linux/macOS, see docs/WEB_EXPORT.md.
 *
 * URL seeding is provided by the in-app test bridge (scripts/testing/TestBridge.cs), which
 * reads ?seed=&algorithm=&x=&y=&z= on load and generates that exact maze. The bridge
 * activates automatically when a `seed` parameter is present, so these URLs need nothing
 * extra. See docs/TEST_BRIDGE.md.
 *
 * The bridge is CONFIRMED working in the patched web export (verified on a real deploy —
 * window.__mazeTestApi === "1", state populated; see docs/TEST_BRIDGE.md), so MAZE_TEST_BRIDGE=1
 * is now set in CI and this suite no longer skips for that reason.
 *
 * It still skips on MAZE_VISUAL_BASELINES, for a different and narrower reason: no baseline PNGs
 * are committed yet. Playwright fails a missing-snapshot test on CI rather than silently creating
 * one, so enabling this before baselines exist would redden every deploy for a reason unrelated
 * to the build. Generating them needs a deploy (the web build cannot be produced on Linux) —
 * see docs/VISUAL_REGRESSION.md for the one-time procedure.
 */

const BRIDGE_PRESENT = process.env.MAZE_TEST_BRIDGE === "1";
const BASELINES_COMMITTED = process.env.MAZE_VISUAL_BASELINES === "1";

/** Fixed cases. Each must render a byte-stable maze given the seeding contract. */
const CASES = [
  { name: "backtracker-10x10", query: "seed=20260725&algorithm=backtracker&x=10&y=10&z=1" },
  { name: "growingtree-10x10", query: "seed=20260725&algorithm=growingtree&x=10&y=10&z=1" },
  { name: "prims-15x15", query: "seed=99&algorithm=prims&x=15&y=15&z=1" },
  { name: "backtracker-3d-8x8x3", query: "seed=7&algorithm=backtracker&x=8&y=8&z=3" },
];

test.describe("maze web build — visual regression", () => {
  test.skip(!process.env.MAZE_URL, "MAZE_URL not set — nothing deployed to screenshot.");
  test.skip(
    !BRIDGE_PRESENT,
    "Test bridge disabled; without URL seeding these screenshots would be nondeterministic.",
  );
  test.skip(
    !BASELINES_COMMITTED,
    "No baseline PNGs committed yet — Playwright fails a missing snapshot on CI, which would " +
      "redden the deploy for a reason unrelated to the build. Generate them from a deploy and " +
      "commit them, then set MAZE_VISUAL_BASELINES=1. See docs/VISUAL_REGRESSION.md.",
  );

  for (const testCase of CASES) {
    test(testCase.name, async ({ page }) => {
      const fatal = failOnRuntimeErrors(page);

      const response = await page.goto(`/?${testCase.query}`, { waitUntil: "domcontentloaded" });
      expect(response?.ok(), `HTTP ${response?.status()} loading the build`).toBeTruthy();

      await waitForEngineBoot(page);
      await waitForStableFrame(page);

      expect(fatal, `fatal runtime error(s):\n${fatal.join("\n")}`).toHaveLength(0);

      // Screenshot the canvas alone, not the page: surrounding chrome (loading bars,
      // fullscreen buttons) is not what we're regression-testing.
      const canvas = page.locator("canvas");
      await expect(canvas).toHaveScreenshot(`${testCase.name}.png`);
    });
  }

  test("same seed reproduces the same render across two independent loads", async ({ browser }) => {
    // Guards the property the whole suite rests on: if the seed isn't actually pinning the
    // render, every other baseline here is untrustworthy.
    //
    // Each load uses a FRESH page. harness.spec.ts measured why: reusing one page across
    // two loads perturbs the raster (~1.6% of pixels differ for identical content), while
    // fresh pages are byte-identical. Compared with the configured threshold rather than
    // byte-exact, because unlike the 2D-canvas harness this is a WebGL/WASM render whose
    // cross-run determinism has not been measured — tighten to byte-exact if it proves
    // stable in practice.
    const shoot = async () => {
      const page = await browser.newPage({ viewport: { width: 1280, height: 720 }, deviceScaleFactor: 1 });
      try {
        await page.goto(`${process.env.MAZE_URL}/?${CASES[0].query}`, { waitUntil: "domcontentloaded" });
        await waitForEngineBoot(page);
        await waitForStableFrame(page);
        await expect(page.locator("canvas")).toHaveScreenshot("seed-stability.png");
      } finally {
        await page.close();
      }
    };

    await shoot();
    await shoot();
  });
});

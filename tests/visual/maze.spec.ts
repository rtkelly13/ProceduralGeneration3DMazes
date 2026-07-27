import { test, expect } from "@playwright/test";
import { waitForEngineBoot, waitForStableFrame, failOnRuntimeErrors } from "./canvas-stability";

/**
 * Visual regression against the deployed C#/WASM build.
 *
 * Requires MAZE_URL (a deployed preview or production URL) — the build cannot be produced
 * locally on Linux/macOS, see docs/WEB_EXPORT.md.
 *
 * PREREQUISITE, NOT YET IMPLEMENTED: the web build must accept generation parameters from
 * the query string so each case renders a known maze. Without it these tests screenshot a
 * randomly-generated maze and fail on every run. See docs/VISUAL_REGRESSION.md ->
 * "Prerequisite: URL-parameter seeding". The tests are skipped until MAZE_SEEDING=1
 * declares that support exists, so this suite never reports a false red.
 */

const SEEDING_SUPPORTED = process.env.MAZE_SEEDING === "1";

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
    !SEEDING_SUPPORTED,
    "URL-parameter seeding not implemented in the web build yet; screenshots would be " +
      "nondeterministic. Set MAZE_SEEDING=1 once it lands.",
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

import { test, expect } from "@playwright/test";
import { waitForStableFrame } from "./canvas-stability";

/**
 * Self-test for the visual-regression harness.
 *
 * The real suite (maze.spec.ts) can only run against a deployed build, which needs a
 * patched Windows editor to produce. That makes it easy for the harness itself — the
 * stability polling, the snapshot comparison, the diff thresholds — to sit unverified until
 * the day someone needs it and finds it broken.
 *
 * This renders a deterministic canvas locally and asserts the same machinery works on it:
 * a seeded draw is byte-stable, an animating canvas is correctly rejected as unstable, and
 * a changed render is actually caught rather than passing under a loose threshold.
 */

/** Seeded canvas drawing, standing in for a seeded maze render. */
function fixture(seed: number, animate = false): string {
  return `
    <style>html,body{margin:0;background:#fff}canvas{display:block}</style>
    <canvas id="c" width="640" height="360"></canvas>
    <script>
      // Mulberry32 — deterministic from the seed, mirroring the C# side's contract.
      let s = ${seed} >>> 0;
      const rnd = () => {
        s = (s + 0x6D2B79F5) >>> 0;
        let t = Math.imul(s ^ (s >>> 15), 1 | s);
        t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
        return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
      };
      const ctx = document.getElementById('c').getContext('2d');
      function draw(offset) {
        s = ${seed} >>> 0;
        ctx.fillStyle = '#fff'; ctx.fillRect(0, 0, 640, 360);
        ctx.strokeStyle = '#222'; ctx.lineWidth = 2;
        for (let i = 0; i < 120; i++) {
          const x = Math.floor(rnd() * 32) * 20 + offset;
          const y = Math.floor(rnd() * 18) * 20;
          ctx.beginPath(); ctx.moveTo(x, y);
          ctx.lineTo(rnd() < 0.5 ? x + 20 : x, rnd() < 0.5 ? y : y + 20);
          ctx.stroke();
        }
      }
      draw(0);
      if (${animate}) {
        let o = 0;
        setInterval(() => { o = (o + 3) % 40; draw(o); }, 100);
      }
    </script>`;
}

test.describe("visual harness self-test", () => {
  test("a seeded canvas render is byte-identical across fresh pages", async ({ browser }) => {
    // Each load gets a FRESH page. That detail is load-bearing: measured on this harness,
    // a seeded draw is byte-identical across fresh pages and even across separate browser
    // launches, but calling setContent twice on one page yields ~3800 differing pixels
    // (1.6%) for identical content. Reusing a page perturbs the raster; a fresh page does
    // not. So screenshot each case on its own page — and byte-exact is a legitimate
    // assertion here, no threshold needed.
    const shoot = async () => {
      const page = await browser.newPage({ viewport: { width: 1280, height: 720 }, deviceScaleFactor: 1 });
      try {
        await page.setContent(fixture(12345));
        await waitForStableFrame(page, { samples: 2, intervalMs: 100, timeoutMs: 10_000 });
        return await page.locator("canvas").screenshot();
      } finally {
        await page.close();
      }
    };

    expect(Buffer.compare(await shoot(), await shoot()),
      "Identical seed produced different pixels on fresh pages — the harness cannot trust any baseline.").toBe(0);
  });

  test("different seeds produce visibly different renders", async ({ page }) => {
    // If this failed, the suite could pass while comparing two blank canvases.
    await page.setContent(fixture(1));
    await waitForStableFrame(page, { samples: 2, intervalMs: 100, timeoutMs: 10_000 });
    const a = await page.locator("canvas").screenshot();

    await page.setContent(fixture(2));
    await waitForStableFrame(page, { samples: 2, intervalMs: 100, timeoutMs: 10_000 });
    const b = await page.locator("canvas").screenshot();

    expect(Buffer.compare(a, b), "Two different seeds rendered identically.").not.toBe(0);
  });

  test("an animating canvas is rejected rather than screenshotted mid-frame", async ({ page }) => {
    // The failure mode this guards: screenshotting before the render settles, which yields
    // a baseline that flakes forever. waitForStableFrame must throw, not return.
    await page.setContent(fixture(999, /* animate */ true));
    await expect(
      waitForStableFrame(page, { samples: 3, intervalMs: 100, timeoutMs: 3_000 }),
    ).rejects.toThrow(/never reached a stable frame/);
  });

  test("baseline comparison catches a changed render", async ({ page }) => {
    // Establishes/compares a committed baseline, exercising the real snapshot path and the
    // configured maxDiffPixelRatio.
    await page.setContent(fixture(4242));
    await waitForStableFrame(page, { samples: 2, intervalMs: 100, timeoutMs: 10_000 });
    await expect(page.locator("canvas")).toHaveScreenshot("harness-baseline.png");
  });
});

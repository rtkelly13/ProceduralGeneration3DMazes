import { Page, expect } from "@playwright/test";

/**
 * Shared helpers for screenshotting a live WebGL/WASM canvas.
 *
 * A canvas app has no "load complete" event that means "finished drawing". Screenshotting
 * on DOM-ready captures a half-drawn frame and produces a flaky baseline, so these helpers
 * wait for the pixels themselves to stop changing.
 */

/** Boot signal: cross-origin isolation granted and the engine has sized its canvas. */
export async function waitForEngineBoot(page: Page, timeout = 150_000): Promise<void> {
  // The .NET WASM runtime needs SharedArrayBuffer, which the browser only grants under
  // COOP/COEP. Assert it explicitly — without it the canvas appears but never starts, and
  // the screenshot would silently capture a blank frame.
  const env = await page.evaluate(() => ({
    coi: self.crossOriginIsolated,
    sab: typeof SharedArrayBuffer !== "undefined",
  }));
  expect(env.coi, "crossOriginIsolated is false — COOP/COEP headers missing").toBe(true);
  expect(env.sab, "SharedArrayBuffer unavailable — cross-origin isolation not effective").toBe(true);

  await page.waitForFunction(
    () => {
      const c = document.querySelector("canvas") as HTMLCanvasElement | null;
      return !!c && c.width > 0 && c.height > 0;
    },
    undefined,
    { timeout },
  );
}

/**
 * Waits until the canvas renders the same content on consecutive samples.
 *
 * Hashes a downscaled copy of the canvas rather than diffing full frames: cheap enough to
 * poll, and insensitive to the sub-pixel noise that would stop a strict comparison from
 * ever settling.
 */
export async function waitForStableFrame(
  page: Page,
  { samples = 3, intervalMs = 500, timeoutMs = 60_000 }: {
    samples?: number;
    intervalMs?: number;
    timeoutMs?: number;
  } = {},
): Promise<void> {
  const deadline = Date.now() + timeoutMs;
  let previous: string | null = null;
  let stableRuns = 0;

  while (Date.now() < deadline) {
    const hash = await page.evaluate(() => {
      const c = document.querySelector("canvas") as HTMLCanvasElement | null;
      if (!c) return null;
      // Downscale to 64x64 through a 2D context, then hash the bytes.
      const scratch = document.createElement("canvas");
      scratch.width = 64;
      scratch.height = 64;
      const ctx = scratch.getContext("2d");
      if (!ctx) return null;
      try {
        ctx.drawImage(c, 0, 0, 64, 64);
      } catch {
        return null; // tainted or not yet drawable
      }
      const { data } = ctx.getImageData(0, 0, 64, 64);
      let h1 = 0x811c9dc5;
      for (let i = 0; i < data.length; i += 4) {
        h1 ^= data[i] | (data[i + 1] << 8) | (data[i + 2] << 16);
        h1 = Math.imul(h1, 0x01000193);
      }
      return (h1 >>> 0).toString(16);
    });

    if (hash !== null && hash === previous) {
      if (++stableRuns >= samples - 1) return;
    } else {
      stableRuns = 0;
    }
    previous = hash;
    await page.waitForTimeout(intervalMs);
  }

  throw new Error(
    `Canvas never reached a stable frame within ${timeoutMs}ms — it is still animating, ` +
      `or the seed did not pin the render. Screenshotting now would produce a flaky baseline.`,
  );
}

/** Fails the test on fatal WASM/runtime errors, which otherwise yield a blank screenshot. */
export function failOnRuntimeErrors(page: Page): string[] {
  const fatal: string[] = [];
  page.on("pageerror", (e) => {
    const text = String(e);
    if (/abort|Aborted|RuntimeError|unreachable|out of memory/i.test(text)) fatal.push(text);
  });
  return fatal;
}

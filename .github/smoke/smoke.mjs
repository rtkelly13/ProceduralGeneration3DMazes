// Post-deploy smoke test: verifies the deployed C#/WASM build actually boots in a
// real browser, not merely that files are served. Fails CI if the hosting regresses.
//
// Usage:  SMOKE_URL=https://… node .github/smoke/smoke.mjs
// Requires: playwright (chromium) installed.

import { chromium } from "playwright";

const url = process.env.SMOKE_URL || process.argv[2];
if (!url) {
  console.error("No URL provided (set SMOKE_URL or pass as argv).");
  process.exit(2);
}

const BOOT_TIMEOUT_MS = 150_000; // ~96 MB payload + .NET WASM init
const fatalErrors = [];

const browser = await chromium.launch();
try {
  const page = await browser.newPage();
  page.on("pageerror", (e) => {
    const t = String(e);
    if (/abort|Aborted|RuntimeError|unreachable|out of memory/i.test(t)) fatalErrors.push(t);
  });

  console.log(`→ Opening ${url}`);
  const resp = await page.goto(url, { waitUntil: "domcontentloaded", timeout: 60_000 });
  if (!resp || !resp.ok()) throw new Error(`HTTP ${resp ? resp.status() : "no response"} for ${url}`);

  // The hosting-critical checks: cross-origin isolation must be granted (COOP/COEP)
  // so the .NET WASM runtime's threads can use SharedArrayBuffer.
  const env = await page.evaluate(() => ({
    coi: self.crossOriginIsolated,
    sab: typeof SharedArrayBuffer !== "undefined",
    hasCanvas: !!document.querySelector("canvas"),
    title: document.title,
  }));
  console.log("  env:", JSON.stringify(env));
  if (!env.coi) throw new Error("crossOriginIsolated is false — COOP/COEP headers missing or wrong.");
  if (!env.sab) throw new Error("SharedArrayBuffer unavailable — cross-origin isolation not effective.");
  if (!env.hasCanvas) throw new Error("No <canvas> element present.");

  // Boot signal: Godot resizes the canvas to the viewport once the engine starts.
  console.log("  waiting for engine to boot (canvas sized)…");
  await page.waitForFunction(
    () => {
      const c = document.querySelector("canvas");
      return c && c.width > 0 && c.height > 0;
    },
    { timeout: BOOT_TIMEOUT_MS },
  );
  const size = await page.evaluate(() => {
    const c = document.querySelector("canvas");
    return { w: c.width, h: c.height };
  });
  console.log("  canvas:", JSON.stringify(size));

  if (fatalErrors.length) {
    throw new Error("Fatal runtime error(s):\n" + fatalErrors.join("\n"));
  }
  console.log("✅ SMOKE PASS");
} finally {
  await browser.close();
}

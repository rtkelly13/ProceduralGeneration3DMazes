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
  //
  // This must NOT be `width > 0 && height > 0`. An untouched <canvas> defaults to 300x150, so
  // that predicate is already true before the engine does anything — it made this check pass in
  // 0.02s on a build that had not booted, which is how it went unnoticed. Assert the canvas is
  // no longer the default size, which only the engine can cause.
  const DEFAULT_CANVAS = { w: 300, h: 150 };
  console.log("  waiting for engine to boot (canvas resized off the 300x150 default)…");
  try {
    await page.waitForFunction(
      (def) => {
        const c = document.querySelector("canvas");
        return !!c && (c.width !== def.w || c.height !== def.h);
      },
      DEFAULT_CANVAS,
      { timeout: BOOT_TIMEOUT_MS },
    );
  } catch {
    const stuck = await page.evaluate(() => {
      const c = document.querySelector("canvas");
      return c ? { w: c.width, h: c.height } : null;
    });
    throw new Error(
      `Engine did not boot: canvas still ${JSON.stringify(stuck)} after ${BOOT_TIMEOUT_MS}ms. ` +
        `A canvas at the 300x150 default means Godot never started — check the browser console ` +
        `output above for a WASM or .NET runtime failure.`,
    );
  }
  const size = await page.evaluate(() => {
    const c = document.querySelector("canvas");
    return { w: c.width, h: c.height };
  });
  console.log("  canvas:", JSON.stringify(size));

  if (fatalErrors.length) {
    throw new Error("Fatal runtime error(s):\n" + fatalErrors.join("\n"));
  }

  // Confirm the deploy is serving the build this run produced, not a stale or partially
  // replaced one. Vercel promotes an alias after upload, so "deploy succeeded" and "the new
  // build is what visitors get" are two different claims — this checks the second.
  //
  // Asserted, not merely reported: unlike the bridge probe below, a wrong build being served
  // is a real user-facing defect, and it is exactly what a deploy check exists to catch.
  const expectedCommit = process.env.EXPECT_COMMIT || "";
  console.log("→ Checking served build identity (build-info.json)");
  const infoUrl = url.replace(/\/+$/, "") + "/build-info.json";
  const infoResp = await page.request.get(infoUrl);
  // Parsed defensively rather than with .json(): a host with an SPA-style catch-all answers an
  // unknown path with index.html and HTTP 200, which would throw a bare SyntaxError here and
  // fail the deploy for the wrong reason. Observed while testing this check.
  let info = null;
  if (!infoResp.ok()) {
    // Predates this file, or the export step changed — worth flagging but not worth failing a
    // deploy over, since nothing user-facing depends on it.
    console.log(`⚠️  build-info.json not served (HTTP ${infoResp.status()}) — cannot verify build identity.`);
  } else {
    const body = await infoResp.text();
    try {
      info = JSON.parse(body);
    } catch {
      console.log(
        `⚠️  build-info.json was not JSON (first 80 chars: ${JSON.stringify(body.slice(0, 80))}) — ` +
          `cannot verify build identity from the sidecar.`,
      );
    }
  }

  if (info) {
    console.log("  build-info.json:", JSON.stringify(info));
    if (!expectedCommit) {
      console.log("  EXPECT_COMMIT not set — reporting the served commit without asserting it.");
    } else if (info.commit !== expectedCommit) {
      throw new Error(
        `Served build is not the one just built.\n` +
          `  expected commit: ${expectedCommit}\n` +
          `  served commit:   ${info.commit || "(empty)"}\n` +
          `The deploy may not have promoted, or an older artefact is still being served.`,
      );
    } else {
      console.log(`✅ Served build matches commit ${expectedCommit}`);
    }
  }

  // Put the identity in the step summary too. A caller inspecting this run (an agent via the
  // GitHub API) can then read one short summary instead of pulling the whole job log.
  if (process.env.GITHUB_STEP_SUMMARY) {
    const rows = info
      ? [
          `| Served commit | \`${info.commit || "(unstamped)"}\` |`,
          `| Built (UTC) | ${info.builtUtc || "—"} |`,
          `| Branch | ${info.branch || "—"} |`,
          `| Editor | ${info.godotEditor || "—"} |`,
        ].join("\n")
      : "| Served commit | _build-info.json unavailable_ |";
    await import("node:fs").then((fs) =>
      fs.appendFileSync(
        process.env.GITHUB_STEP_SUMMARY,
        `### Served build\n\n| | |\n|---|---|\n${rows}\n| Expected | \`${expectedCommit || "(not asserted)"}\` |\n`,
      ),
    );
  }

  // Report whether the in-app test bridge came up under this (patched-template) build.
  // Reported, not asserted: the bridge is an automation aid, and a build without it is still
  // a working build — failing the smoke test over it would block deploys for no user-visible
  // reason. But it is the one thing unit and scene tests cannot answer (see
  // docs/TEST_BRIDGE.md -> "Verification status"), so every deploy answers it for free
  // instead of waiting for someone to check by hand.
  const bridgeUrl = url + (url.includes("?") ? "&" : "?") + "test=1";
  console.log(`→ Checking test bridge at ${bridgeUrl}`);
  const probe = await browser.newPage();
  try {
    await probe.goto(bridgeUrl, { waitUntil: "domcontentloaded", timeout: 60_000 });
    await probe.waitForFunction(() => self.crossOriginIsolated === true, { timeout: 60_000 });
    const ready = await probe
      .waitForFunction(() => window.__mazeTestApi === "1", { timeout: BOOT_TIMEOUT_MS })
      .then(() => true)
      .catch(() => false);

    if (ready) {
      const state = await probe.evaluate(() => window.__mazeState ?? null);
      console.log("✅ TEST BRIDGE PRESENT — set MAZE_TEST_BRIDGE=1 to enable the browser suites");
      console.log("   window.__mazeState:", String(state).slice(0, 300));

      // Cross-check the commit compiled INTO the WASM against the one in the sidecar JSON.
      // They come from the same inputs, so a disagreement means the two halves of the deploy
      // are from different builds — which a check of either one alone would miss.
      try {
        const inApp = JSON.parse(state || "{}").buildCommit || "";
        if (expectedCommit && inApp && inApp !== expectedCommit) {
          throw new Error(
            `The running WASM reports commit ${inApp}, but this run built ${expectedCommit}.`,
          );
        }
        console.log(`   in-app buildCommit: ${inApp || "(unstamped)"}`);
      } catch (e) {
        if (e instanceof SyntaxError) console.log("   (could not parse __mazeState)");
        else throw e;
      }
      if (process.env.GITHUB_STEP_SUMMARY) {
        await import("node:fs").then((fs) =>
          fs.appendFileSync(
            process.env.GITHUB_STEP_SUMMARY,
            "### Test bridge: **present** ✅\n\nSet `MAZE_TEST_BRIDGE=1` in `web-export.yml` " +
              "to un-skip the Playwright functional and visual suites.\n",
          ),
        );
      }
    } else {
      console.log("⚠️  TEST BRIDGE ABSENT — window.__mazeTestApi never became \"1\".");
      console.log("   The deploy is fine; the browser suites stay skipped. See docs/TEST_BRIDGE.md.");
      if (process.env.GITHUB_STEP_SUMMARY) {
        await import("node:fs").then((fs) =>
          fs.appendFileSync(
            process.env.GITHUB_STEP_SUMMARY,
            "### Test bridge: **absent** ⚠️\n\n`window.__mazeTestApi` never became `\"1\"`. " +
              "Keep `MAZE_TEST_BRIDGE=0`. Check the Godot console for a `TestBridge: enabled` " +
              "line — see docs/TEST_BRIDGE.md.\n",
          ),
        );
      }
    }
  } finally {
    await probe.close();
  }

  console.log("✅ SMOKE PASS");
} finally {
  await browser.close();
}

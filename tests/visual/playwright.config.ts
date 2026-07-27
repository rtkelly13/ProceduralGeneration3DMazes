import { defineConfig, devices } from "@playwright/test";

/**
 * Visual regression config for the maze web build.
 *
 * Two projects:
 *  - `maze`            — screenshots the deployed build. Needs MAZE_URL.
 *  - `harness-selftest`— screenshots a local deterministic canvas fixture. Proves the
 *                        comparison harness itself works without needing a Godot export,
 *                        which matters because producing the web build requires a patched
 *                        Windows editor (see docs/WEB_EXPORT.md).
 *
 * Snapshots are platform-keyed by Playwright. Baselines MUST be generated on the same
 * platform CI uses (linux) or every run fails on a missing snapshot — see
 * docs/VISUAL_REGRESSION.md.
 */
/**
 * Optional escape hatch for environments that ship a pinned Chromium whose build number
 * doesn't match this @playwright/test version (sandboxes, air-gapped runners). GitHub
 * Actions runs `playwright install` and needs none of this — leave it unset there.
 */
const launchOptions = process.env.CHROMIUM_PATH
  ? { executablePath: process.env.CHROMIUM_PATH }
  : {};

export default defineConfig({
  testDir: ".",
  // Canvas/WASM boot is slow: ~96 MB payload plus .NET runtime init.
  timeout: 180_000,
  expect: {
    toHaveScreenshot: {
      // Canvas rendering is not bit-identical across driver/GPU revisions even on the
      // same image, so allow a small ratio rather than demanding zero diff. Tight enough
      // that a changed maze (thousands of differing pixels) still fails.
      maxDiffPixelRatio: 0.01,
      // Ignore sub-perceptual per-pixel noise from antialiasing.
      threshold: 0.2,
      animations: "disabled",
    },
  },
  // Visual baselines are order- and load-sensitive; keep it serial and retry-free so a
  // failure means a real diff rather than a flake masked by a retry.
  workers: 1,
  retries: 0,
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  reporter: process.env.CI
    ? [["github"], ["html", { open: "never" }], ["list"]]
    : [["html", { open: "never" }], ["list"]],
  use: {
    // Fixed viewport: a different window size is a different screenshot.
    viewport: { width: 1280, height: 720 },
    // Deterministic rendering across machines.
    deviceScaleFactor: 1,
    colorScheme: "light",
    timezoneId: "UTC",
    locale: "en-GB",
    screenshot: "only-on-failure",
    trace: "retain-on-failure",
  },
  projects: [
    {
      name: "maze",
      testMatch: /maze\.spec\.ts/,
      use: {
        ...devices["Desktop Chrome"],
        baseURL: process.env.MAZE_URL,
        launchOptions,
      },
    },
    {
      name: "harness-selftest",
      testMatch: /harness\.spec\.ts/,
      use: { ...devices["Desktop Chrome"], launchOptions },
    },
  ],
});

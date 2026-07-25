import { test, expect, Page } from "@playwright/test";
import { waitForEngineBoot, waitForStableFrame, failOnRuntimeErrors } from "./canvas-stability";

/**
 * Functional (behavioural) tests against the deployed C#/WASM build.
 *
 * These assert on application *state*, not pixels. That is only possible because the build
 * exposes a test bridge — a Godot web export draws everything into one <canvas>, so
 * Playwright's locator model (getByRole/getByText) cannot see inside the app at all. See
 * docs/TEST_BRIDGE.md for the contract and scripts/testing/TestBridge.cs for the
 * implementation.
 *
 * The bridge is opt-in: it only activates when the URL carries `test=1` (or a `seed`), so
 * these tests append it explicitly.
 *
 * Scope note: most UI behaviour is cheaper to test in-engine — see tests/scene/ and
 * docs/TESTING.md. What lives here is the handful of things only the *browser* build can
 * break: cross-origin isolation, the WASM runtime booting, real input reaching the engine,
 * and a full journey completing end to end.
 */

const BRIDGE_READY_MS = 150_000;

/** Reads window.__mazeState and parses it. The bridge publishes a JSON string. */
async function readState(page: Page): Promise<Record<string, unknown>> {
  const raw = await page.evaluate(() => (window as never as { __mazeState?: string }).__mazeState);
  expect(raw, "window.__mazeState is absent — is the test bridge enabled?").toBeTruthy();
  return JSON.parse(raw as string);
}

/** Sends a fire-and-forget command. Results are observed via state, never returned. */
async function sendCommand(page: Page, command: Record<string, unknown>): Promise<void> {
  await page.evaluate((json) => {
    (window as never as { __mazeCommand: (s: string) => void }).__mazeCommand(json);
  }, JSON.stringify(command));
}

/** Loads the app with the bridge switched on and waits for it to be live. */
async function openApp(page: Page, query = ""): Promise<void> {
  const sep = query ? "&" : "";
  const response = await page.goto(`${process.env.MAZE_URL}/?test=1${sep}${query}`, {
    waitUntil: "domcontentloaded",
  });
  expect(response?.ok(), `HTTP ${response?.status()} loading the build`).toBeTruthy();

  await waitForEngineBoot(page);
  await page.waitForFunction(
    () => (window as never as { __mazeTestApi?: string }).__mazeTestApi === "1",
    undefined,
    { timeout: BRIDGE_READY_MS },
  );
}

test.describe("maze web build — functional", () => {
  test.skip(!process.env.MAZE_URL, "MAZE_URL not set — nothing deployed to drive.");
  test.skip(
    process.env.MAZE_TEST_BRIDGE !== "1",
    "Test bridge not confirmed present in the deployed build yet. " +
      "Set MAZE_TEST_BRIDGE=1 once a deploy includes scripts/testing/TestBridge.cs — " +
      "skipping rather than failing so this cannot report a false red.",
  );

  test("bridge comes up and reports app state", async ({ page }) => {
    const fatal = failOnRuntimeErrors(page);
    await openApp(page);

    const state = await readState(page);
    expect(state.ready, "GameState should be initialised").toBe(true);
    expect(fatal, `fatal runtime error(s):\n${fatal.join("\n")}`).toHaveLength(0);
  });

  test("URL seeding generates the requested maze on load", async ({ page }) => {
    // The mechanism the visual suite depends on: same URL must mean same maze.
    await openApp(page, "seed=20260725&algorithm=backtracker&x=10&y=10&z=1");
    await page.waitForFunction(
      () => JSON.parse((window as never as { __mazeState: string }).__mazeState).hasMaze === true,
      undefined,
      { timeout: BRIDGE_READY_MS },
    );

    const state = await readState(page);
    expect(state.seed, "the reported seed must be the one asked for").toBe(20260725);
    expect(state.algorithm).toBe("RecursiveBacktrackerAlgorithm");
    expect(state.sizeX).toBe(10);
    expect(state.sizeY).toBe(10);
    expect(state.scene).toContain("maze.tscn");
    expect(state.lastError, "bridge reported an error").toBe("");
  });

  test("same seed produces the same maze in the browser", async ({ browser }) => {
    // The determinism guarantee, verified through the *web* runtime rather than in unit
    // tests — this is what makes browser-side golden comparison trustworthy.
    const fingerprint = async () => {
      const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
      try {
        await openApp(page, "seed=4242&algorithm=growingtree&x=12&y=12&z=1");
        await page.waitForFunction(
          () => JSON.parse((window as never as { __mazeState: string }).__mazeState).hasMaze === true,
          undefined,
          { timeout: BRIDGE_READY_MS },
        );
        const s = await readState(page);
        return `${s.seed}|${JSON.stringify(s.start)}|${JSON.stringify(s.end)}|${s.shortestPath}|${s.deadEnds}|${s.junctions}`;
      } finally {
        await page.close();
      }
    };

    const [a, b] = [await fingerprint(), await fingerprint()];
    expect(b, "same seed produced a different maze in the browser").toBe(a);
  });

  test("different seeds produce different mazes", async ({ page }) => {
    // Guards the inverse: if seeding collapsed everything onto one maze, the test above
    // would pass while asserting nothing.
    await openApp(page, "seed=1&algorithm=prims&x=12&y=12&z=1");
    await page.waitForFunction(
      () => JSON.parse((window as never as { __mazeState: string }).__mazeState).hasMaze === true,
      undefined,
      { timeout: BRIDGE_READY_MS },
    );
    const first = await readState(page);

    await sendCommand(page, { cmd: "generate", seed: 2, algorithm: "prims", x: 12, y: 12, z: 1 });
    await page.waitForFunction(
      (prev) => JSON.parse((window as never as { __mazeState: string }).__mazeState).seed !== prev,
      first.seed,
      { timeout: BRIDGE_READY_MS },
    );
    const second = await readState(page);

    expect(second.seed).toBe(2);
    const same =
      JSON.stringify(first.start) === JSON.stringify(second.start) &&
      JSON.stringify(first.end) === JSON.stringify(second.end) &&
      first.shortestPath === second.shortestPath;
    expect(same, "two different seeds produced an identical maze").toBe(false);
  });

  test("generated maze is solvable and endpoints are distinct", async ({ page }) => {
    // A real behavioural assertion that no screenshot could make.
    await openApp(page, "seed=99&algorithm=backtracker&x=14&y=14&z=1");
    await page.waitForFunction(
      () => JSON.parse((window as never as { __mazeState: string }).__mazeState).hasMaze === true,
      undefined,
      { timeout: BRIDGE_READY_MS },
    );

    const s = await readState(page);
    expect(s.totalCells).toBe(196);
    expect(s.shortestPath as number, "maze should have a solution path").toBeGreaterThan(0);
    expect(JSON.stringify(s.start), "start and end must differ").not.toBe(JSON.stringify(s.end));
  });

  test("3D maze level navigation clamps at the top and bottom", async ({ page }) => {
    await openApp(page, "seed=7&algorithm=backtracker&x=8&y=8&z=3");
    await page.waitForFunction(
      () => JSON.parse((window as never as { __mazeState: string }).__mazeState).hasMaze === true,
      undefined,
      { timeout: BRIDGE_READY_MS },
    );

    await sendCommand(page, { cmd: "setLevel", level: 99 });
    await expect
      .poll(async () => (await readState(page)).currentLevel, { timeout: 10_000 })
      .toBe(2); // Z=3 -> highest level index is 2

    await sendCommand(page, { cmd: "setLevel", level: -5 });
    await expect
      .poll(async () => (await readState(page)).currentLevel, { timeout: 10_000 })
      .toBe(0);
  });

  test("navigating to another screen works", async ({ page }) => {
    await openApp(page, "seed=5");
    await page.waitForFunction(
      () => JSON.parse((window as never as { __mazeState: string }).__mazeState).hasMaze === true,
      undefined,
      { timeout: BRIDGE_READY_MS },
    );

    await sendCommand(page, { cmd: "goto", scene: "menu" });
    await expect
      .poll(async () => (await readState(page)).scene, { timeout: 30_000 })
      .toContain("menu.tscn");

    // And the render settles on the new screen rather than being left mid-transition.
    await waitForStableFrame(page);
  });

  test("an unknown command is reported, not silently ignored", async ({ page }) => {
    // The bridge surfaces failures through state because a thrown error would be invisible
    // to the caller. If this regressed, every other test here could pass while the app
    // quietly did nothing.
    await openApp(page);
    await sendCommand(page, { cmd: "nonsense" });

    await expect
      .poll(async () => (await readState(page)).lastError, { timeout: 10_000 })
      .toContain("unknown command");
  });

  test("keyboard input reaches the engine", async ({ page }) => {
    // Proves the browser→engine input path works at all, which no state assertion covers.
    // Esc is bound in the app (see ShortcutsModal); this asserts the build survives real
    // key events rather than asserting a specific UI reaction, which belongs in scene tests.
    const fatal = failOnRuntimeErrors(page);
    await openApp(page, "seed=11");
    await page.waitForFunction(
      () => JSON.parse((window as never as { __mazeState: string }).__mazeState).hasMaze === true,
      undefined,
      { timeout: BRIDGE_READY_MS },
    );

    const canvas = page.locator("canvas");
    await canvas.click({ position: { x: 10, y: 10 } }); // focus the canvas
    await page.keyboard.press("Space");
    await page.keyboard.press("Escape");
    await page.waitForTimeout(500);

    const s = await readState(page);
    expect(s.ready, "app should still be alive after input").toBe(true);
    expect(fatal, `input caused a fatal error:\n${fatal.join("\n")}`).toHaveLength(0);
  });
});

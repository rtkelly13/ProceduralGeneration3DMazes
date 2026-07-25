using NUnit.Framework;

// Enable parallel test execution at the assembly level
// ParallelScope.All runs test fixtures and their children in parallel
[assembly: Parallelizable(ParallelScope.All)]

// One fixture instance per test case. Not a performance choice — it is what makes
// ParallelScope.All above *correct*.
//
// Measured under this project's NUnit (3.14) with ParallelScope.All: NUnit creates ONE fixture
// instance and runs its test cases on several worker threads, so multiple `[SetUp]` bodies are
// in flight against the same instance simultaneously. Every field a `[SetUp]` assigns is
// therefore a data race between tests of the same fixture.
//
// The resulting failure points nowhere near the cause. `RandomValueTests` does:
//
//   _mazePointFactory = new Mock<IMazePointFactory>();                  // (a) publishes a BARE mock
//   _mazePointFactory.Setup(x => x.MakePoint(...)).Returns(...);        // (b) configures the field's mock
//   _randomPoint = new RandomPointGenerator(_random, _mazePointFactory.Object);  // (c) RE-READS the field
//
// If another test's (a) lands between this test's (b) and (c), then (c) captures that other
// thread's unconfigured mock. A loose Moq mock returns default(MazePoint) — null, because
// MazePoint is a class — so `RandomPoint` hands back null and the test dies dereferencing
// `point.X` inside an assertion loop. That is exactly how it presented: two NullReferenceExceptions
// in RandomValueTests on CI, on a commit whose other CI runs were green and which passed every
// local run. `MovementHelperTests` was marked `[NonParallelizable]` for the same root cause.
//
// InstancePerTestCase gives each test its own fixture instance (verified: four concurrent tests,
// four distinct instances), so no `[SetUp]` can observe another's half-built state. Parallelism is
// kept — the unsafe sharing is not.
//
// Constraint this imposes: `[OneTimeSetUp]`/`[OneTimeTearDown]` must be static under this
// lifecycle. There are none in this assembly, and any added later must be static.
[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]

namespace ProceduralMaze.Tests;

/// <summary>
/// Guards the assembly-level test lifecycle above.
/// </summary>
/// <remarks>
/// Without this, deleting <c>[assembly: FixtureLifeCycle(...)]</c> reintroduces a race whose only
/// symptom is an occasional NullReferenceException in an unrelated-looking test — the kind of
/// regression that gets re-diagnosed from scratch months later, or dismissed as "CI being flaky".
/// This turns that into an immediate, named failure.
/// </remarks>
[TestFixture]
public class TestLifecycleGuardTests
{
    [Test]
    public void Assembly_RunsOneFixtureInstancePerTestCase()
    {
        var attribute = typeof(TestLifecycleGuardTests).Assembly
            .GetCustomAttributes(typeof(FixtureLifeCycleAttribute), false)
            .Cast<FixtureLifeCycleAttribute>()
            .SingleOrDefault();

        Assert.That(attribute, Is.Not.Null,
            "The test assembly must declare [assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]. "
            + "Without it, ParallelScope.All runs test cases concurrently against a single shared "
            + "fixture instance, making every field assigned in [SetUp] a data race. See TestSetup.cs.");

        Assert.That(attribute!.LifeCycle, Is.EqualTo(LifeCycle.InstancePerTestCase),
            "SingleInstance is unsafe in combination with ParallelScope.All. See TestSetup.cs.");
    }
}

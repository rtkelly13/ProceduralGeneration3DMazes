using NUnit.Framework;
using ProceduralMaze.Maze.Solver;
using ProceduralMaze.UI;

namespace ProceduralMaze.Tests;

/// <summary>
/// Covers <see cref="AnimationController"/> — a 254-line playback state machine that had **no
/// tests at all**, not because it was hard to test but because it lives in <c>scripts/ui/</c>,
/// which this project didn't compile.
///
/// It has no Godot dependency (no <c>using Godot;</c>) and takes time as a
/// <c>Update(deltaTime)</c> parameter rather than reading a clock, so it is directly testable
/// with no engine, no fakes and no waiting. See docs/TESTING.md.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AnimationControllerTests
{
    /// <summary>Playback duration per step at speed 1.0, from AnimationController.</summary>
    private const float BaseStepDuration = 0.5f;

    private static List<AlgorithmStep> Steps(int count) =>
        Enumerable.Range(0, count)
            .Select(i => new AlgorithmStep { Type = StepType.SelectNode, Description = $"step {i}" })
            .ToList();

    #region Initial state

    [Test]
    public void NewController_StartsStoppedAtTheFirstStep()
    {
        var c = new AnimationController(Steps(5));

        Assert.Multiple(() =>
        {
            Assert.That(c.State, Is.EqualTo(PlaybackState.Stopped));
            Assert.That(c.CurrentStepIndex, Is.Zero);
            Assert.That(c.TotalSteps, Is.EqualTo(5));
            Assert.That(c.IsAtStart, Is.True);
            Assert.That(c.IsAtEnd, Is.False);
            Assert.That(c.CurrentStep?.Description, Is.EqualTo("step 0"));
        });
    }

    [Test]
    public void NullSteps_AreTreatedAsEmpty()
    {
        // The constructor coalesces null, so this must not throw on construction or use.
        var c = new AnimationController(null!);

        Assert.Multiple(() =>
        {
            Assert.That(c.TotalSteps, Is.Zero);
            Assert.That(c.CurrentStep, Is.Null);
            Assert.DoesNotThrow(() => c.Update(1f));
            Assert.DoesNotThrow(c.Play);
        });
    }

    [Test]
    public void EmptySteps_CannotBePlayed()
    {
        var c = new AnimationController(Steps(0));
        c.Play();

        // Play() bails on an empty list, so state must stay Stopped rather than Playing
        // forever with nothing to advance through.
        Assert.That(c.State, Is.EqualTo(PlaybackState.Stopped));
    }

    #endregion

    #region Transport controls

    [Test]
    public void Play_ThenPause_ThenPlay_RestoresPlayingState()
    {
        var c = new AnimationController(Steps(3));

        c.Play();
        Assert.That(c.State, Is.EqualTo(PlaybackState.Playing));
        c.Pause();
        Assert.That(c.State, Is.EqualTo(PlaybackState.Paused));
        c.Play();
        Assert.That(c.State, Is.EqualTo(PlaybackState.Playing));
    }

    [Test]
    public void Pause_WhenNotPlaying_IsANoOp()
    {
        var c = new AnimationController(Steps(3));
        c.Pause();

        Assert.That(c.State, Is.EqualTo(PlaybackState.Stopped),
            "pausing a stopped controller should not change its state");
    }

    [Test]
    public void TogglePlayPause_AlternatesStates()
    {
        var c = new AnimationController(Steps(3));

        c.TogglePlayPause();
        Assert.That(c.State, Is.EqualTo(PlaybackState.Playing));
        c.TogglePlayPause();
        Assert.That(c.State, Is.EqualTo(PlaybackState.Paused));
        c.TogglePlayPause();
        Assert.That(c.State, Is.EqualTo(PlaybackState.Playing));
    }

    [Test]
    public void Stop_ResetsToTheBeginning()
    {
        var c = new AnimationController(Steps(5));
        c.Play();
        c.StepForward();
        c.StepForward();

        c.Stop();

        Assert.Multiple(() =>
        {
            Assert.That(c.State, Is.EqualTo(PlaybackState.Stopped));
            Assert.That(c.CurrentStepIndex, Is.Zero);
            Assert.That(c.IsAtStart, Is.True);
        });
    }

    #endregion

    #region Stepping

    [Test]
    public void StepForward_StopsAtTheLastStep()
    {
        var c = new AnimationController(Steps(3));

        c.StepForward();
        c.StepForward();
        c.StepForward();   // already at the end
        c.StepForward();

        Assert.Multiple(() =>
        {
            Assert.That(c.CurrentStepIndex, Is.EqualTo(2), "must not run past the last index");
            Assert.That(c.IsAtEnd, Is.True);
        });
    }

    [Test]
    public void StepBackward_StopsAtTheFirstStep()
    {
        var c = new AnimationController(Steps(3));
        c.StepForward();

        c.StepBackward();
        c.StepBackward();  // already at the start

        Assert.Multiple(() =>
        {
            Assert.That(c.CurrentStepIndex, Is.Zero, "must not go below zero");
            Assert.That(c.IsAtStart, Is.True);
        });
    }

    [TestCase(0)]
    [TestCase(2)]
    [TestCase(4)]
    public void GoToStep_JumpsToValidIndex(int index)
    {
        var c = new AnimationController(Steps(5));
        c.GoToStep(index);
        Assert.That(c.CurrentStepIndex, Is.EqualTo(index));
    }

    [TestCase(-1)]
    [TestCase(5)]
    [TestCase(999)]
    public void GoToStep_IgnoresOutOfRangeIndex(int index)
    {
        var c = new AnimationController(Steps(5));
        c.GoToStep(2);

        c.GoToStep(index);

        Assert.That(c.CurrentStepIndex, Is.EqualTo(2),
            "an out-of-range jump should leave the position untouched, not clamp or throw");
    }

    #endregion

    #region Time-driven playback

    [Test]
    public void Update_DoesNothingWhileNotPlaying()
    {
        var c = new AnimationController(Steps(5));

        c.Update(10f);   // far beyond a step duration

        Assert.That(c.CurrentStepIndex, Is.Zero, "a stopped controller must not advance");
    }

    [Test]
    public void Update_AdvancesOneStepPerStepDuration()
    {
        var c = new AnimationController(Steps(5));
        c.Play();

        c.Update(BaseStepDuration * 0.9f);
        Assert.That(c.CurrentStepIndex, Is.Zero, "should not advance before a full duration");

        c.Update(BaseStepDuration * 0.2f);   // cumulative 1.1x — crosses the threshold
        Assert.That(c.CurrentStepIndex, Is.EqualTo(1), "elapsed time should accumulate");
    }

    [Test]
    public void Update_AdvancesOnlyOnceEvenForAVeryLargeDelta()
    {
        // Documents real behaviour: the timer resets after a single StepForward, so a long
        // frame does not fast-forward several steps. Worth pinning — a future change to
        // "catch up" would be a behaviour change, not a refactor.
        var c = new AnimationController(Steps(10));
        c.Play();

        c.Update(BaseStepDuration * 5);

        Assert.That(c.CurrentStepIndex, Is.EqualTo(1));
    }

    [Test]
    public void Update_ReachesFinishedAtTheEnd()
    {
        var c = new AnimationController(Steps(3));
        c.Play();

        for (var i = 0; i < 5; i++)
        {
            c.Update(BaseStepDuration);
        }

        Assert.Multiple(() =>
        {
            Assert.That(c.State, Is.EqualTo(PlaybackState.Finished));
            Assert.That(c.IsAtEnd, Is.True);
        });
    }

    [Test]
    public void Play_AfterFinishing_RestartsFromTheBeginning()
    {
        var c = new AnimationController(Steps(3));
        c.Play();
        for (var i = 0; i < 5; i++)
        {
            c.Update(BaseStepDuration);
        }
        Assert.That(c.State, Is.EqualTo(PlaybackState.Finished), "precondition");

        c.Play();

        Assert.Multiple(() =>
        {
            Assert.That(c.CurrentStepIndex, Is.Zero, "replay should rewind");
            Assert.That(c.State, Is.EqualTo(PlaybackState.Playing));
        });
    }

    [Test]
    public void HigherPlaybackSpeed_AdvancesSooner()
    {
        var slow = new AnimationController(Steps(5), 1.0f);
        var fast = new AnimationController(Steps(5), 2.0f);
        slow.Play();
        fast.Play();

        // Half of the base duration: enough at 2x, not enough at 1x.
        slow.Update(BaseStepDuration / 2);
        fast.Update(BaseStepDuration / 2);

        Assert.Multiple(() =>
        {
            Assert.That(slow.CurrentStepIndex, Is.Zero, "1x should not have advanced yet");
            Assert.That(fast.CurrentStepIndex, Is.EqualTo(1), "2x should have advanced");
        });
    }

    #endregion

    #region Speed

    [TestCase(3.0f, 3.0f)]
    [TestCase(0.05f, 0.1f)]   // clamped up to the minimum
    [TestCase(99f, 5.0f)]     // clamped down to the maximum
    [TestCase(-1f, 0.1f)]
    public void PlaybackSpeed_IsClampedToTheSupportedRange(float set, float expected)
    {
        var c = new AnimationController(Steps(3)) { PlaybackSpeed = set };
        Assert.That(c.PlaybackSpeed, Is.EqualTo(expected).Within(0.0001f));
    }

    [Test]
    public void SpeedUp_AndSlowDown_MoveInQuarterStepsAndStopAtTheLimits()
    {
        var c = new AnimationController(Steps(3), 1.0f);

        c.SpeedUp();
        Assert.That(c.PlaybackSpeed, Is.EqualTo(1.25f).Within(0.0001f));
        c.SlowDown();
        Assert.That(c.PlaybackSpeed, Is.EqualTo(1.0f).Within(0.0001f));

        for (var i = 0; i < 50; i++)
        {
            c.SpeedUp();
        }
        Assert.That(c.PlaybackSpeed, Is.EqualTo(5.0f).Within(0.0001f), "must cap at 5x");

        for (var i = 0; i < 100; i++)
        {
            c.SlowDown();
        }
        Assert.That(c.PlaybackSpeed, Is.EqualTo(0.1f).Within(0.0001f), "must floor at 0.1x");
    }

    #endregion

    #region Notifications and formatting

    [Test]
    public void StepChanged_FiresOnEveryPositionChange()
    {
        var c = new AnimationController(Steps(4));
        var seen = new List<string>();
        c.StepChanged += s => seen.Add(s.Description);

        c.StepForward();     // -> step 1
        c.StepForward();     // -> step 2
        c.StepBackward();    // -> step 1
        c.GoToStep(3);       // -> step 3
        c.Stop();            // -> step 0

        Assert.That(seen, Is.EqualTo(new[] { "step 1", "step 2", "step 1", "step 3", "step 0" }));
    }

    [Test]
    public void StepChanged_DoesNotFireWhenAStepIsRefused()
    {
        var c = new AnimationController(Steps(2));
        c.GoToStep(1);
        var fired = 0;
        c.StepChanged += _ => fired++;

        c.StepForward();     // already at the end
        c.GoToStep(99);      // out of range

        Assert.That(fired, Is.Zero, "refused moves should not notify listeners");
    }

    [Test]
    public void ProgressAndSpeedStrings_AreHumanReadableAndOneBased()
    {
        var c = new AnimationController(Steps(10), 1.5f);
        c.GoToStep(3);

        Assert.Multiple(() =>
        {
            Assert.That(c.GetProgressString(), Is.EqualTo("Step 4/10"), "progress is 1-based");
            Assert.That(c.GetSpeedString(), Is.EqualTo("1.5x"));
        });
    }

    [Test]
    public void SpeedString_UsesInvariantFormatting()
    {
        // The web build forces InvariantGlobalization; a comma decimal separator here would
        // mean desktop and browser disagreed. Pinning it costs nothing.
        var c = new AnimationController(Steps(2), 2.5f);
        Assert.That(c.GetSpeedString(), Does.Contain("."));
    }

    #endregion
}

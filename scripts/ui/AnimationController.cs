using System;
using System.Collections.Generic;
using ProceduralMaze.Maze.Solver;

namespace ProceduralMaze.UI
{
    /// <summary>
    /// Playback state for algorithm animation.
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// Animation is stopped at the beginning.
        /// </summary>
        Stopped,

        /// <summary>
        /// Animation is currently playing.
        /// </summary>
        Playing,

        /// <summary>
        /// Animation is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// Animation has reached the end.
        /// </summary>
        Finished
    }

    /// <summary>
    /// Controls playback of algorithm animation steps.
    /// </summary>
    public class AnimationController
    {
        private readonly List<AlgorithmStep> _steps;
        private int _currentStepIndex;
        private float _playbackSpeed;
        private float _timeSinceLastStep;
        private const float BaseStepDuration = 0.5f; // seconds per step at speed 1.0

        /// <summary>
        /// Current playback state.
        /// </summary>
        public PlaybackState State { get; private set; } = PlaybackState.Stopped;

        /// <summary>
        /// Current step index (0-based).
        /// </summary>
        public int CurrentStepIndex => _currentStepIndex;

        /// <summary>
        /// Total number of steps.
        /// </summary>
        public int TotalSteps => _steps.Count;

        /// <summary>
        /// Current playback speed multiplier.
        /// </summary>
        public float PlaybackSpeed
        {
            get => _playbackSpeed;
            set => _playbackSpeed = Math.Clamp(value, 0.1f, 5.0f);
        }

        /// <summary>
        /// Gets the current step, or null if no steps exist.
        /// </summary>
        public AlgorithmStep? CurrentStep => _steps.Count > 0 && _currentStepIndex < _steps.Count
            ? _steps[_currentStepIndex]
            : null;

        /// <summary>
        /// Whether the animation is at the first step.
        /// </summary>
        public bool IsAtStart => _currentStepIndex == 0;

        /// <summary>
        /// Whether the animation is at the last step.
        /// </summary>
        public bool IsAtEnd => _currentStepIndex >= _steps.Count - 1;

        /// <summary>
        /// Event fired when the current step changes.
        /// </summary>
        public event Action<AlgorithmStep>? StepChanged;

        /// <summary>
        /// Creates a new animation controller with the given steps.
        /// </summary>
        public AnimationController(List<AlgorithmStep> steps, float initialSpeed = 1.0f)
        {
            _steps = steps ?? new List<AlgorithmStep>();
            _currentStepIndex = 0;
            _playbackSpeed = initialSpeed;
            _timeSinceLastStep = 0;
        }

        /// <summary>
        /// Updates the animation state. Call this every frame with delta time.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (State != PlaybackState.Playing || _steps.Count == 0)
                return;

            _timeSinceLastStep += deltaTime;

            float stepDuration = BaseStepDuration / _playbackSpeed;
            if (_timeSinceLastStep >= stepDuration)
            {
                _timeSinceLastStep = 0;
                StepForward();

                if (IsAtEnd)
                {
                    State = PlaybackState.Finished;
                }
            }
        }

        /// <summary>
        /// Starts or resumes playback.
        /// </summary>
        public void Play()
        {
            if (_steps.Count == 0) return;

            if (State == PlaybackState.Finished)
            {
                // Restart from beginning if finished
                _currentStepIndex = 0;
            }

            State = PlaybackState.Playing;
            _timeSinceLastStep = 0;
        }

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public void Pause()
        {
            if (State == PlaybackState.Playing)
            {
                State = PlaybackState.Paused;
            }
        }

        /// <summary>
        /// Toggles between play and pause.
        /// </summary>
        public void TogglePlayPause()
        {
            if (State == PlaybackState.Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        /// <summary>
        /// Stops playback and resets to the beginning.
        /// </summary>
        public void Stop()
        {
            State = PlaybackState.Stopped;
            _currentStepIndex = 0;
            _timeSinceLastStep = 0;
            NotifyStepChanged();
        }

        /// <summary>
        /// Advances to the next step.
        /// </summary>
        public void StepForward()
        {
            if (_currentStepIndex < _steps.Count - 1)
            {
                _currentStepIndex++;
                NotifyStepChanged();
            }
        }

        /// <summary>
        /// Goes back to the previous step.
        /// </summary>
        public void StepBackward()
        {
            if (_currentStepIndex > 0)
            {
                _currentStepIndex--;
                NotifyStepChanged();
            }
        }

        /// <summary>
        /// Jumps to a specific step index.
        /// </summary>
        public void GoToStep(int stepIndex)
        {
            if (stepIndex >= 0 && stepIndex < _steps.Count)
            {
                _currentStepIndex = stepIndex;
                NotifyStepChanged();
            }
        }

        /// <summary>
        /// Increases playback speed.
        /// </summary>
        public void SpeedUp()
        {
            PlaybackSpeed = Math.Min(PlaybackSpeed + 0.25f, 5.0f);
        }

        /// <summary>
        /// Decreases playback speed.
        /// </summary>
        public void SlowDown()
        {
            PlaybackSpeed = Math.Max(PlaybackSpeed - 0.25f, 0.1f);
        }

        /// <summary>
        /// Gets a formatted string showing current progress.
        /// </summary>
        public string GetProgressString()
        {
            return $"Step {_currentStepIndex + 1}/{_steps.Count}";
        }

        /// <summary>
        /// Gets a formatted string showing current speed.
        /// </summary>
        public string GetSpeedString()
        {
            return $"{_playbackSpeed:F1}x";
        }

        private void NotifyStepChanged()
        {
            if (CurrentStep != null)
            {
                StepChanged?.Invoke(CurrentStep);
            }
        }
    }
}

using Godot;

namespace ProceduralMaze.UI
{
    /// <summary>
    /// Configuration settings for path visualization features.
    /// Settings reset to defaults each time the scene loads.
    /// </summary>
    public class PathVisualizationSettings
    {
        #region Alternative Paths Settings

        /// <summary>
        /// Maximum number of alternative paths to compute (1-10).
        /// </summary>
        public int MaxAlternativePaths { get; set; } = 5;

        /// <summary>
        /// Whether to show all paths simultaneously with transparency.
        /// </summary>
        public bool ShowAllPathsSimultaneously { get; set; } = false;

        /// <summary>
        /// Whether to highlight where paths diverge from each other.
        /// </summary>
        public bool ShowPathDifference { get; set; } = true;

        #endregion

        #region Animation Settings

        /// <summary>
        /// Whether animation mode is currently enabled.
        /// </summary>
        public bool AnimationEnabled { get; set; } = false;

        /// <summary>
        /// Default playback speed (1.0 = normal).
        /// </summary>
        public float DefaultPlaybackSpeed { get; set; } = 1.0f;

        /// <summary>
        /// Whether to show step descriptions during animation.
        /// </summary>
        public bool ShowStepDescription { get; set; } = true;

        /// <summary>
        /// Whether to highlight frontier nodes during animation.
        /// </summary>
        public bool HighlightFrontier { get; set; } = true;

        /// <summary>
        /// Whether to show distance updates during animation.
        /// </summary>
        public bool ShowDistanceUpdates { get; set; } = true;

        #endregion

        #region Decision Point Settings

        /// <summary>
        /// Current detail level for decision points.
        /// </summary>
        public DecisionDetailLevel DecisionDetailLevel { get; set; } = DecisionDetailLevel.Off;

        /// <summary>
        /// Whether to show branching factor badges.
        /// </summary>
        public bool ShowBranchingFactor { get; set; } = true;

        /// <summary>
        /// Whether to show choice reasoning at decision points.
        /// </summary>
        public bool ShowChoiceReasoning { get; set; } = true;

        /// <summary>
        /// Whether to show rejected paths with dim styling.
        /// </summary>
        public bool HighlightRejectedPaths { get; set; } = false;

        #endregion

        #region Colors - Paths

        /// <summary>
        /// Color for the optimal (shortest) path.
        /// </summary>
        public Color OptimalPathColor { get; set; } = new Color(0.2f, 0.6f, 1.0f); // Blue

        /// <summary>
        /// Colors for alternative paths (cycled through).
        /// </summary>
        public Color[] AlternativePathColors { get; set; } = new[]
        {
            new Color(1.0f, 0.8f, 0.2f),  // Yellow
            new Color(1.0f, 0.5f, 0.0f),  // Orange
            new Color(0.8f, 0.2f, 0.8f),  // Purple
            new Color(0.2f, 0.8f, 0.8f),  // Cyan
        };

        #endregion

        #region Colors - Animation

        /// <summary>
        /// Color for visited (processed) nodes.
        /// </summary>
        public Color VisitedNodeColor { get; set; } = new Color(0.4f, 0.4f, 0.4f);

        /// <summary>
        /// Color for frontier (in queue with finite distance) nodes.
        /// </summary>
        public Color FrontierNodeColor { get; set; } = new Color(1.0f, 1.0f, 0.2f); // Yellow

        /// <summary>
        /// Color for the currently selected node.
        /// </summary>
        public Color CurrentNodeColor { get; set; } = new Color(0.2f, 1.0f, 0.2f); // Bright green

        /// <summary>
        /// Color for edge currently being examined.
        /// </summary>
        public Color ExaminedEdgeColor { get; set; } = new Color(1.0f, 1.0f, 0.0f); // Yellow

        /// <summary>
        /// Color for edge that was relaxed (distance improved).
        /// </summary>
        public Color RelaxedEdgeColor { get; set; } = new Color(0.2f, 1.0f, 0.2f); // Green

        #endregion

        #region Colors - Decision Points

        /// <summary>
        /// Color for decision point highlight.
        /// </summary>
        public Color DecisionPointColor { get; set; } = new Color(1.0f, 0.8f, 0.0f); // Gold

        /// <summary>
        /// Scale multiplier for decision point nodes.
        /// </summary>
        public float DecisionPointScale { get; set; } = 1.2f;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the color for a path by its index (0 = optimal, 1+ = alternatives).
        /// </summary>
        public Color GetPathColor(int pathIndex)
        {
            if (pathIndex == 0)
            {
                return OptimalPathColor;
            }

            int altIndex = (pathIndex - 1) % AlternativePathColors.Length;
            return AlternativePathColors[altIndex];
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        public void ResetToDefaults()
        {
            MaxAlternativePaths = 5;
            ShowAllPathsSimultaneously = false;
            ShowPathDifference = true;

            AnimationEnabled = false;
            DefaultPlaybackSpeed = 1.0f;
            ShowStepDescription = true;
            HighlightFrontier = true;
            ShowDistanceUpdates = true;

            DecisionDetailLevel = DecisionDetailLevel.Off;
            ShowBranchingFactor = true;
            ShowChoiceReasoning = true;
            HighlightRejectedPaths = false;
        }

        #endregion
    }

    /// <summary>
    /// Detail level for decision point visualization.
    /// </summary>
    public enum DecisionDetailLevel
    {
        /// <summary>
        /// Decision points not highlighted.
        /// </summary>
        Off,

        /// <summary>
        /// Show branching factor badges only.
        /// </summary>
        Badges,

        /// <summary>
        /// Show full detail including rejected edge costs.
        /// </summary>
        Detailed
    }
}

using Godot;
using System.Collections.Generic;

namespace ProceduralMaze.UI
{
    /// <summary>
    /// Toast notification types with associated colors.
    /// </summary>
    public enum ToastType
    {
        Success,
        Error,
        Info
    }

    /// <summary>
    /// A self-contained toast notification component.
    /// Shows messages that auto-dismiss after a configurable duration.
    /// Positioned at bottom-center of the screen, above the BottomPanel.
    /// </summary>
    public partial class ToastNotification : Control
    {
        // Configuration
        private const float DefaultDuration = 4.0f;
        private const float FadeInDuration = 0.2f;
        private const float FadeOutDuration = 0.3f;
        private const float BottomOffset = 100f; // Above BottomPanel
        private const float MaxWidth = 600f;
        private const float Padding = 16f;

        // Colors for different toast types
        private static readonly Color SuccessBackground = new(0.15f, 0.5f, 0.15f, 0.95f);
        private static readonly Color ErrorBackground = new(0.6f, 0.15f, 0.15f, 0.95f);
        private static readonly Color InfoBackground = new(0.15f, 0.35f, 0.55f, 0.95f);
        private static readonly Color TextColor = new(1f, 1f, 1f, 1f);
        private static readonly Color DetailColor = new(0.8f, 0.8f, 0.8f, 0.8f);

        // UI elements
        private PanelContainer _panel = null!;
        private VBoxContainer _container = null!;
        private Label _messageLabel = null!;
        private Label _detailLabel = null!;
        private StyleBoxFlat _styleBox = null!;

        // State
        private readonly Queue<ToastData> _queue = new();
        private bool _isShowing;
        private float _displayTimer;
        private float _currentDuration;
        private ToastState _state = ToastState.Hidden;

        private enum ToastState
        {
            Hidden,
            FadingIn,
            Visible,
            FadingOut
        }

        private record ToastData(string Message, string? Details, ToastType Type, float Duration);

        public override void _Ready()
        {
            CreateUI();
            Hide();
        }

        private void CreateUI()
        {
            // Create the panel
            _panel = new PanelContainer();
            _panel.Name = "ToastPanel";
            AddChild(_panel);

            // Create and configure the style
            _styleBox = new StyleBoxFlat();
            _styleBox.BgColor = InfoBackground;
            _styleBox.SetCornerRadiusAll(8);
            _styleBox.SetContentMarginAll(Padding);
            _panel.AddThemeStyleboxOverride("panel", _styleBox);

            // Create the container
            _container = new VBoxContainer();
            _container.AddThemeConstantOverride("separation", 4);
            _panel.AddChild(_container);

            // Create message label
            _messageLabel = new Label();
            _messageLabel.Name = "MessageLabel";
            _messageLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _messageLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _messageLabel.CustomMinimumSize = new Vector2(200, 0);
            _messageLabel.AddThemeColorOverride("font_color", TextColor);
            _messageLabel.AddThemeFontSizeOverride("font_size", 16);
            _container.AddChild(_messageLabel);

            // Create detail label (hidden by default)
            _detailLabel = new Label();
            _detailLabel.Name = "DetailLabel";
            _detailLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _detailLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _detailLabel.AddThemeColorOverride("font_color", DetailColor);
            _detailLabel.AddThemeFontSizeOverride("font_size", 12);
            _detailLabel.Visible = false;
            _container.AddChild(_detailLabel);

            // Set anchors for bottom-center positioning
            AnchorLeft = 0.5f;
            AnchorRight = 0.5f;
            AnchorTop = 1.0f;
            AnchorBottom = 1.0f;
            GrowHorizontal = GrowDirection.Both;
            GrowVertical = GrowDirection.Begin;

            // Ensure we don't block mouse input when hidden
            MouseFilter = MouseFilterEnum.Ignore;
        }

        public override void _Process(double delta)
        {
            if (_state == ToastState.Hidden)
            {
                // Check if there are queued toasts
                if (_queue.Count > 0)
                {
                    ShowNextToast();
                }
                return;
            }

            var dt = (float)delta;

            switch (_state)
            {
                case ToastState.FadingIn:
                    _displayTimer += dt;
                    var fadeInProgress = Mathf.Clamp(_displayTimer / FadeInDuration, 0f, 1f);
                    Modulate = new Color(1, 1, 1, fadeInProgress);
                    
                    // Slide up effect
                    var startY = BottomOffset - 20;
                    var endY = BottomOffset;
                    OffsetBottom = -Mathf.Lerp(startY, endY, fadeInProgress);
                    
                    if (_displayTimer >= FadeInDuration)
                    {
                        _state = ToastState.Visible;
                        _displayTimer = 0;
                        Modulate = Colors.White;
                    }
                    break;

                case ToastState.Visible:
                    _displayTimer += dt;
                    if (_displayTimer >= _currentDuration)
                    {
                        _state = ToastState.FadingOut;
                        _displayTimer = 0;
                    }
                    break;

                case ToastState.FadingOut:
                    _displayTimer += dt;
                    var fadeOutProgress = Mathf.Clamp(_displayTimer / FadeOutDuration, 0f, 1f);
                    Modulate = new Color(1, 1, 1, 1 - fadeOutProgress);
                    
                    if (_displayTimer >= FadeOutDuration)
                    {
                        _state = ToastState.Hidden;
                        _isShowing = false;
                        Visible = false;
                    }
                    break;
            }
        }

        /// <summary>
        /// Shows a toast notification.
        /// </summary>
        /// <param name="message">The main message to display.</param>
        /// <param name="type">The type of toast (affects color).</param>
        /// <param name="details">Optional technical details (shown in smaller text).</param>
        /// <param name="duration">How long to show the toast (default 4 seconds).</param>
        public void Show(string message, ToastType type, string? details = null, float duration = DefaultDuration)
        {
            var toast = new ToastData(message, details, type, duration);
            
            if (_isShowing)
            {
                // Queue the toast if one is already showing
                _queue.Enqueue(toast);
            }
            else
            {
                DisplayToast(toast);
            }
        }

        /// <summary>
        /// Shows a success toast.
        /// </summary>
        public void ShowSuccess(string message, string? details = null)
        {
            Show(message, ToastType.Success, details);
        }

        /// <summary>
        /// Shows an error toast.
        /// </summary>
        public void ShowError(string message, string? details = null)
        {
            // Errors show a bit longer
            Show(message, ToastType.Error, details, 6.0f);
        }

        /// <summary>
        /// Shows an info toast.
        /// </summary>
        public void ShowInfo(string message, string? details = null)
        {
            Show(message, ToastType.Info, details);
        }

        private void ShowNextToast()
        {
            if (_queue.Count > 0)
            {
                var toast = _queue.Dequeue();
                DisplayToast(toast);
            }
        }

        private void DisplayToast(ToastData toast)
        {
            _isShowing = true;
            _state = ToastState.FadingIn;
            _displayTimer = 0;
            _currentDuration = toast.Duration;

            // Set content
            _messageLabel.Text = toast.Message;
            
            if (!string.IsNullOrEmpty(toast.Details))
            {
                _detailLabel.Text = toast.Details;
                _detailLabel.Visible = true;
            }
            else
            {
                _detailLabel.Visible = false;
            }

            // Set color based on type
            _styleBox.BgColor = toast.Type switch
            {
                ToastType.Success => SuccessBackground,
                ToastType.Error => ErrorBackground,
                ToastType.Info => InfoBackground,
                _ => InfoBackground
            };

            // Calculate panel size and position
            _panel.Size = new Vector2(0, 0); // Reset to recalculate
            _panel.ResetSize();

            // Ensure the panel doesn't exceed max width
            if (_panel.Size.X > MaxWidth)
            {
                _messageLabel.CustomMinimumSize = new Vector2(MaxWidth - Padding * 2, 0);
                _detailLabel.CustomMinimumSize = new Vector2(MaxWidth - Padding * 2, 0);
                _panel.ResetSize();
            }

            // Center the panel horizontally
            _panel.Position = new Vector2(-_panel.Size.X / 2, 0);

            // Position at bottom of screen
            OffsetBottom = -BottomOffset + 20; // Start slightly below for slide-up effect
            OffsetTop = OffsetBottom - _panel.Size.Y;
            OffsetLeft = 0;
            OffsetRight = 0;

            // Start invisible for fade in
            Modulate = new Color(1, 1, 1, 0);
            Visible = true;
        }

        /// <summary>
        /// Immediately dismisses the current toast.
        /// </summary>
        public void Dismiss()
        {
            if (_state == ToastState.Visible || _state == ToastState.FadingIn)
            {
                _state = ToastState.FadingOut;
                _displayTimer = 0;
            }
        }

        /// <summary>
        /// Clears all queued toasts.
        /// </summary>
        public void ClearQueue()
        {
            _queue.Clear();
        }
    }
}

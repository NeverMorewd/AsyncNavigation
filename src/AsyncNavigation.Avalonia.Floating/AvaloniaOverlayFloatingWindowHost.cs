using AsyncNavigation.Floating;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace AsyncNavigation.Avalonia.Floating;

/// <summary>
/// Renders a floating "window" as a draggable, resizable in-page overlay panel instead of a
/// native OS window. Selected automatically for Avalonia's single-view application lifetime
/// (browser/WASM, mobile), where there is no windowing system to host a real <see cref="Window"/>.
/// </summary>
internal sealed class AvaloniaOverlayFloatingWindowHost : IFloatingWindowHost
{
    private const double DefaultWidth = 400;
    private const double DefaultHeight = 300;
    private const double MinWidth = 200;
    private const double MinHeight = 120;
    private const double CascadeStep = 28;
    private const int CascadeSlots = 10;

    private static int _cascadeIndex;
    private static int _nextZIndex = 1;

    private readonly Control _mainView;
    private readonly ContentControl _content = new();
    private readonly Border _panel;

    private OverlayLayer? _layer;
    private bool _dragging;
    private Point _dragStartPointerPosition;
    private Point _dragStartPanelPosition;
    private bool _resizing;
    private Point _resizeStartPointerPosition;
    private Size _resizeStartSize;

    public AvaloniaOverlayFloatingWindowHost(FloatingWindowOptions options, Control mainView)
    {
        _mainView = mainView;

        var titleText = new TextBlock
        {
            Text = options.Title,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
            FontWeight = FontWeight.SemiBold
        };

        var dockButton = new Button { Content = "Dock", Margin = new Thickness(4, 2) };
        dockButton.Click += (_, _) => RestoreRequested?.Invoke(this, EventArgs.Empty);

        // A plain ASCII glyph, not "✕": the default WASM/browser font set doesn't cover it and
        // renders tofu instead.
        var closeButton = new Button { Content = "X", Margin = new Thickness(0, 2, 4, 2) };
        closeButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        var titleBarButtons = new StackPanel { Orientation = Orientation.Horizontal };
        titleBarButtons.Children.Add(dockButton);
        titleBarButtons.Children.Add(closeButton);
        Grid.SetColumn(titleBarButtons, 1);

        var titleBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Background = Brushes.Gainsboro,
            Cursor = new Cursor(StandardCursorType.SizeAll)
        };
        titleBar.Children.Add(titleText);
        titleBar.Children.Add(titleBarButtons);
        titleBar.PointerPressed += OnTitleBarPointerPressed;
        titleBar.PointerMoved += OnTitleBarPointerMoved;
        titleBar.PointerReleased += OnTitleBarPointerReleased;
        Grid.SetRow(titleBar, 0);

        Grid.SetRow(_content, 1);

        var body = new Grid { RowDefinitions = new RowDefinitions("Auto,*") };
        body.Children.Add(titleBar);
        body.Children.Add(_content);

        var resizeGrip = new Border
        {
            Width = 14,
            Height = 14,
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.BottomRightCorner),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        resizeGrip.PointerPressed += OnResizeGripPointerPressed;
        resizeGrip.PointerMoved += OnResizeGripPointerMoved;
        resizeGrip.PointerReleased += OnResizeGripPointerReleased;

        var root = new Grid();
        root.Children.Add(body);
        root.Children.Add(resizeGrip);

        _panel = new Border
        {
            Width = options.Width ?? DefaultWidth,
            Height = options.Height ?? DefaultHeight,
            BorderBrush = Brushes.DimGray,
            BorderThickness = new Thickness(1),
            Background = Brushes.White,
            Child = root
        };
    }

    public event EventHandler? RestoreRequested;
    public event EventHandler? CloseRequested;

    public Task SetContentAsync(object? content, CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _content.Content = content, cancellationToken);

    public Task ShowAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(() =>
        {
            var topLevel = TopLevel.GetTopLevel(_mainView)
                ?? throw new InvalidOperationException(
                    "Could not resolve a TopLevel for the current view to host floating content in.");
            _layer = OverlayLayer.GetOverlayLayer(topLevel)
                ?? throw new InvalidOperationException("Could not find an overlay layer to host floating content in.");

            if (!_layer.Children.Contains(_panel))
            {
                var offset = (_cascadeIndex++ % CascadeSlots) * CascadeStep;
                Canvas.SetLeft(_panel, offset);
                Canvas.SetTop(_panel, offset);
                _layer.Children.Add(_panel);
            }
            BringToFront();
        }, cancellationToken);

    public Task ActivateAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(() =>
        {
            BringToFront();
            _panel.Focus();
        }, cancellationToken);

    public Task CloseAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _layer?.Children.Remove(_panel), cancellationToken);

    public ValueTask DisposeAsync()
    {
        var task = InvokeAsync(() => _layer?.Children.Remove(_panel), CancellationToken.None);
        return new ValueTask(task);
    }

    private void BringToFront() => _panel.ZIndex = _nextZIndex++;

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_layer is null || !e.GetCurrentPoint(_panel).Properties.IsLeftButtonPressed) return;
        _dragging = true;
        _dragStartPointerPosition = e.GetPosition(_layer);
        _dragStartPanelPosition = new Point(Canvas.GetLeft(_panel), Canvas.GetTop(_panel));
        e.Pointer.Capture((IInputElement)sender!);
        BringToFront();
        e.Handled = true;
    }

    private void OnTitleBarPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging || _layer is null) return;
        var delta = e.GetPosition(_layer) - _dragStartPointerPosition;
        var maxLeft = Math.Max(0, _layer.Bounds.Width - _panel.Bounds.Width);
        var maxTop = Math.Max(0, _layer.Bounds.Height - _panel.Bounds.Height);
        Canvas.SetLeft(_panel, Math.Clamp(_dragStartPanelPosition.X + delta.X, 0, maxLeft));
        Canvas.SetTop(_panel, Math.Clamp(_dragStartPanelPosition.Y + delta.Y, 0, maxTop));
    }

    private void OnTitleBarPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        e.Pointer.Capture(null);
    }

    private void OnResizeGripPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_layer is null || !e.GetCurrentPoint(_panel).Properties.IsLeftButtonPressed) return;
        _resizing = true;
        _resizeStartPointerPosition = e.GetPosition(_layer);
        _resizeStartSize = new Size(_panel.Width, _panel.Height);
        e.Pointer.Capture((IInputElement)sender!);
        BringToFront();
        e.Handled = true;
    }

    private void OnResizeGripPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_resizing || _layer is null) return;
        var delta = e.GetPosition(_layer) - _resizeStartPointerPosition;
        _panel.Width = Math.Max(MinWidth, _resizeStartSize.Width + delta.X);
        _panel.Height = Math.Max(MinHeight, _resizeStartSize.Height + delta.Y);
    }

    private void OnResizeGripPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_resizing) return;
        _resizing = false;
        e.Pointer.Capture(null);
    }

    private static async Task InvokeAsync(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }
        await Dispatcher.UIThread.InvokeAsync(action);
    }
}

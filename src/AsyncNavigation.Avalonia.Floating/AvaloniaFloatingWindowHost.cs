using AsyncNavigation.Floating;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Threading;

namespace AsyncNavigation.Avalonia.Floating;

internal sealed class AvaloniaFloatingWindowHostFactory : IFloatingWindowHostFactory
{
    public IFloatingWindowHost Create(FloatingWindowOptions options)
    {
        if (!Dispatcher.UIThread.CheckAccess())
            throw new InvalidOperationException("Floating windows must be created on the Avalonia UI thread.");

        // Classic desktop hosts get a real OS window; everything else (browser/WASM, mobile
        // single-view apps) has no windowing system, so floating content is rendered as an
        // in-page overlay panel instead.
        return Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime => new AvaloniaFloatingWindowHost(options),
            ISingleViewApplicationLifetime { MainView: { } mainView } => new AvaloniaOverlayFloatingWindowHost(options, mainView),
            ISingleViewApplicationLifetime => throw new InvalidOperationException(
                "The single-view application lifetime does not have a MainView to host floating content in."),
            _ => throw new NotSupportedException(
                "Floating windows require Avalonia's classic desktop or single-view application lifetime.")
        };
    }
}

internal sealed class AvaloniaFloatingWindowHost : IFloatingWindowHost
{
    private readonly Window _window;
    private bool _allowClose;
    private readonly ContentControl _content = new();

    public AvaloniaFloatingWindowHost(FloatingWindowOptions options)
    {
        _window = new Window
        {
            Title = options.Title,
            Topmost = options.Topmost,
            SizeToContent = options.Width.HasValue || options.Height.HasValue
                ? SizeToContent.Manual
                : SizeToContent.WidthAndHeight
        };
        if (options.Width.HasValue) _window.Width = options.Width.Value;
        if (options.Height.HasValue) _window.Height = options.Height.Value;
        var panel = new DockPanel { LastChildFill = true };
        if (options.ShowDockButton)
        {
            var dockButton = new Button
            {
                Content = "Dock to region",
                Height = 30,
                Margin = new Thickness(8),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            dockButton.Click += (_, _) => RestoreRequested?.Invoke(this, EventArgs.Empty);
            DockPanel.SetDock(dockButton, Dock.Top);
            panel.Children.Add(dockButton);
        }
        panel.Children.Add(_content);
        _window.Content = panel;
        _window.Closing += OnClosing;
    }

    public event EventHandler? RestoreRequested;
    public event EventHandler? CloseRequested;

    public Task SetContentAsync(object? content, CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _content.Content = content, cancellationToken);

    public Task ShowAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(_window.Show, cancellationToken);

    public Task ActivateAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _window.Activate(), cancellationToken);

    public Task CloseAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(() =>
        {
            _allowClose = true;
            _window.Close();
        }, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await InvokeAsync(() =>
        {
            _window.Closing -= OnClosing;
            if (_window.IsVisible)
            {
                _allowClose = true;
                _window.Close();
            }
        }, CancellationToken.None);
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose)
            return;

        e.Cancel = true;
        Dispatcher.UIThread.Post(
            () => CloseRequested?.Invoke(this, EventArgs.Empty),
            DispatcherPriority.Normal);
    }

    private async Task InvokeAsync(Action action, CancellationToken cancellationToken)
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

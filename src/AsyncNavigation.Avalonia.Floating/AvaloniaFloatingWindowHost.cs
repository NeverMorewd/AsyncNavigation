using AsyncNavigation.Floating;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace AsyncNavigation.Avalonia.Floating;

internal sealed class AvaloniaFloatingWindowHostFactory : IFloatingWindowHostFactory
{
    public IFloatingWindowHost Create(FloatingWindowOptions options)
    {
        if (!Dispatcher.UIThread.CheckAccess())
            throw new InvalidOperationException("Floating windows must be created on the Avalonia UI thread.");
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime)
            throw new NotSupportedException("Floating windows require Avalonia's classic desktop lifetime.");
        return new AvaloniaFloatingWindowHost(options);
    }
}

internal sealed class AvaloniaFloatingWindowHost : IFloatingWindowHost
{
    private readonly Window _window;
    private bool _allowClose;

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
        _window.Closing += OnClosing;
    }

    public event EventHandler? RestoreRequested;

    public Task SetContentAsync(object? content, CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _window.Content = content, cancellationToken);

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
        RestoreRequested?.Invoke(this, EventArgs.Empty);
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

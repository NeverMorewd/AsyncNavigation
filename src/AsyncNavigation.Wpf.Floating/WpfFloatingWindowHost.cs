using AsyncNavigation.Floating;
using System.ComponentModel;
using System.Windows;

namespace AsyncNavigation.Wpf.Floating;

internal sealed class WpfFloatingWindowHostFactory : IFloatingWindowHostFactory
{
    public IFloatingWindowHost Create(FloatingWindowOptions options)
    {
        var application = Application.Current
            ?? throw new InvalidOperationException("A WPF Application must be running before a floating window can be created.");
        if (!application.Dispatcher.CheckAccess())
            throw new InvalidOperationException("Floating windows must be created on the WPF UI thread.");
        return new WpfFloatingWindowHost(options);
    }
}

internal sealed class WpfFloatingWindowHost : IFloatingWindowHost
{
    private readonly Window _window;
    private bool _allowClose;

    public WpfFloatingWindowHost(FloatingWindowOptions options)
    {
        _window = new Window
        {
            Title = options.Title ?? string.Empty,
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

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_allowClose)
            return;
        e.Cancel = true;
        RestoreRequested?.Invoke(this, EventArgs.Empty);
    }

    private Task InvokeAsync(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_window.Dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }
        return _window.Dispatcher.InvokeAsync(action).Task;
    }
}

using AsyncNavigation;
using AsyncNavigation.Abstractions;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using System;
using System.Threading.Tasks;

namespace Sample.Avalonia;

internal class NotifyIndicatorProvider : IRegionIndicatorProvider
{
    private readonly NotifyIndicator _notifyIndicator;
    private readonly IServiceProvider _serviceProvider;
    public NotifyIndicatorProvider(IServiceProvider serviceProvider)
    {
        _notifyIndicator = new();
        _serviceProvider = serviceProvider;
    }

    public IRegionIndicator GetIndicator(string regionName)
    {
        return _notifyIndicator;
    }

    public bool HasIndicator(string regionName)
    {
        return true;
    }
}

internal class NotifyIndicator : IRegionIndicator
{
    private WindowNotificationManager? _notificationLoadingManager;
    private WindowNotificationManager? _notificationErrorManager;
    private bool _inited = false;
    public NotifyIndicator()
    {

    }
    Task IRegionIndicator.OnCancelledAsync(NavigationContext context)
    {
        _notificationLoadingManager?.CloseAll();
        return Task.CompletedTask;
    }
    public Task OnLoadedAsync(NavigationContext context)
    {
        _notificationLoadingManager?.CloseAll();
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(NavigationContext context, Exception? innerException = null)
    {
        InitNotificationManager();
        _notificationErrorManager!.Show(new Notification { Message = context.ToString() }, NotificationType.Error, TimeSpan.FromSeconds(3));
        return Task.CompletedTask;
    }

    public Task ShowLoadingAsync(NavigationContext context)
    {
        InitNotificationManager();
        _notificationLoadingManager!.Show(new Notification { Message = context.ToString() }, NotificationType.Information, TimeSpan.FromSeconds(3));
        return Task.CompletedTask;
    }
    private void InitNotificationManager()
    {
        if (_inited)
        {
            return;
        }
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopStyleApplicationLifetime)
        {
            _notificationLoadingManager = new WindowNotificationManager(desktopStyleApplicationLifetime.MainWindow)
            {
                Position = NotificationPosition.TopCenter
            };
            _notificationErrorManager = new WindowNotificationManager(desktopStyleApplicationLifetime.MainWindow)
            {
                Position = NotificationPosition.TopCenter
            };
        }
        else
        {
            //
            _notificationLoadingManager = new WindowNotificationManager();
            _notificationErrorManager = new WindowNotificationManager();
        }
        _inited = true;
    }
}

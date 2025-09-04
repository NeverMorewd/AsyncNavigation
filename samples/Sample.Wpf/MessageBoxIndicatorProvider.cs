using AsyncNavigation;
using AsyncNavigation.Abstractions;
using System.Windows;

namespace Sample.Avalonia;

internal class MessageBoxIndicatorProvider : IRegionIndicatorProvider
{
    private readonly MessageBoxIndicator _indicator;
    private readonly IServiceProvider _serviceProvider;
    public MessageBoxIndicatorProvider(IServiceProvider serviceProvider)
    {
        _indicator = new();
        _serviceProvider = serviceProvider;
    }

    public IRegionIndicator GetIndicator(string regionName)
    {
        return _indicator;
    }

    public bool HasIndicator(string regionName)
    {
        return true;
    }
}

internal class MessageBoxIndicator : IRegionIndicator
{
    public Task ShowErrorAsync(NavigationContext context, Exception? innerException = null)
    {
        return ShowInNewThread(() =>
        {
            var window = new ErrorWindow
            {
                Title = "Error",
                Content = $"{context}{Environment.NewLine}Error: {innerException}",
                Foreground = System.Windows.Media.Brushes.Red
            };
            window.Closed += (s, e) => System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);
            window.Show();
        });
    }

    public Task ShowLoadingAsync(NavigationContext context)
    {
        return ShowInNewThreadWithoutWait(() =>
        {
            var window = new LoadingWindow
            {
                Title = "Loading",
                Content = context.ToString()
            };
            window.Closed += (s, e) => System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);
            window.Show();
        });
    }

    private Task ShowInNewThread(Action showAction)
    {
        var tcs = new TaskCompletionSource<object?>();

        var thread = new Thread(() =>
        {
            try
            {
                showAction();
                System.Windows.Threading.Dispatcher.Run();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();

        return tcs.Task;
    }

    private Task ShowInNewThreadWithoutWait(Action showAction)
    {
        var thread = new Thread(() =>
        {
            try
            {
                showAction();
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (Exception)
            {
                
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        return Task.CompletedTask;
    }
}

public class ErrorWindow : Window
{
    public ErrorWindow()
    {
        Width = 400;
        Height = 200;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}

public class LoadingWindow : Window
{
    public LoadingWindow()
    {
        Width = 300;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}



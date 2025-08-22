using AsyncNavigation.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sample.Avalonia.Views;
using Sample.Common;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sample.Avalonia;

public partial class App : Application, IObserver<Exception>
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        #region error handle
        Dispatcher.UIThread.UnhandledException += (s, e) =>
        {
            HandleError(e.Exception, true);
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            HandleError((e.ExceptionObject as Exception)!, e.IsTerminating);
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            e.SetObserved();
            HandleError(e.Exception);
        };
        RxApp.DefaultExceptionHandler = this;
        #endregion

        var services = new ServiceCollection();
        services.AddNavigationSupport()
                .AddSingleton<MainWindowViewModel>()
                .RegisterNavigation<AView, AViewModel>(nameof(AView))
                .RegisterNavigation<BView, BViewModel>(nameof(BView))
                .RegisterNavigation<CView, CViewModel>(nameof(CView))
                .RegisterNavigation<DView, DViewModel>(nameof(DView))
                .RegisterNavigation<EView, EViewModel>(nameof(EView))
                .RegisterLoadingIndicator(BuildLoadingIndicator)
                .RegisterErrorIndicator(BuildErrorIndicator);

        #region setup lifetime
        var sp = services.BuildServiceProvider();
        var viewModel = sp.GetRequiredService<MainWindowViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = viewModel
            };
        }
        #endregion

        base.OnFrameworkInitializationCompleted();
    }

    public void OnNext(Exception value)
    {
        HandleError(value);
    }
    public void OnCompleted()
    {
        //
    }

    public void OnError(Exception error)
    {
        HandleError(error);
    }

    private void HandleError(Exception error, bool needThrow = false)
    {
        Debug.WriteLine($"HandleError:{error}");
        if (needThrow)
            throw error;
    }

    private Control BuildLoadingIndicator(IServiceProvider sp, NavigationContext navigationContext)
    {
        var textLoading = new TextBlock
        {
            Text = "Loaing...",
            FontSize = 20,
            Foreground = Brushes.Orange,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        var text = new TextBlock
        {
            Text = navigationContext.ToString(),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        var bar = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 30,
        };
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.Add(textLoading);
        panel.Children.Add(text);
        panel.Children.Add(bar);
        var border = new Border
        {
            Child = panel,
            Padding = new Thickness(5),
            Margin = new Thickness(5),
            Background = Brushes.White,
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.AliceBlue,
        };
        return border;
    }

    private Control BuildErrorIndicator(IServiceProvider sp, NavigationContext navigationContext)
    {
        var textFailed = new TextBlock
        {
            Text = "Failed",
            FontSize = 20,
            Foreground = Brushes.Red,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        DockPanel.SetDock(textFailed, Dock.Top);
        var error = new SelectableTextBlock
        {
            Text = navigationContext.ToString(),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        var scrollView = new ScrollViewer
        {
            Content = error,
        };
        var panel = new DockPanel
        {
            LastChildFill = true,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.Add(textFailed);
        panel.Children.Add(scrollView);
        var border = new Border
        {
            Child = panel,
            Padding = new Thickness(5),
            Margin = new Thickness(5),
            Background = Brushes.White,
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.AliceBlue,
        };
        return border;
    }
}
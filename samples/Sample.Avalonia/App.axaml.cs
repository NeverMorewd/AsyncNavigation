using AsyncNavigation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Sample.Avalonia.Regions;
using Sample.Avalonia.Views;
using Sample.Common;
using System;

namespace Sample.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        NavigationOptions navigationOptions = new()
        {
            /// default is CancelCurrent <see cref="AsyncNavigation.Core.NavigationJobStrategy.CancelCurrent"/>
            //NavigationJobStrategy = AsyncNavigation.Core.NavigationJobStrategy.Queue
        };


        var services = new ServiceCollection();
        services.AddNavigationSupport(navigationOptions)
                .AddSingleton<MainWindowViewModel>()
                .RegisterView<AView, AViewModel>(nameof(AView))
                .RegisterView<BView, BViewModel>(nameof(BView))
                .RegisterView<CView, CViewModel>(nameof(CView))
                .RegisterView<DView, DViewModel>(nameof(DView))
                .RegisterView<EView, EViewModel>(nameof(EView))
                .RegisterView<ListBoxRegionView, ListBoxRegionViewModel>(nameof(ListBoxRegionView))
                .RegisterRegionIndicatorProvider<NotifyIndicatorProvider>()
                .RegisterLoadingIndicator(BuildLoadingIndicator)
                .RegisterErrorIndicator(BuildErrorIndicator)
                .RegisterRegionAdapter<ListBoxRegionAdapter>();

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
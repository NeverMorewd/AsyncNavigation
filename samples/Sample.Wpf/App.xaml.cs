using AsyncNavigation;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sample.Common;
using Sample.Wpf.Regions;
using Sample.Wpf.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sample.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Wpf);
            var services = new ServiceCollection();
            services.AddNavigationSupport()
                .AddSingleton<MainWindowViewModel>()
                    .RegisterRegionAdapter<ListBoxRegionAdapter>()
                    .RegisterView<AView, AViewModel>(nameof(AView))
                    .RegisterView<BView, BViewModel>(nameof(BView))
                    .RegisterView<CView, CViewModel>(nameof(CView))
                    .RegisterView<DView, DViewModel>(nameof(DView))
                    .RegisterView<EView, EViewModel>(nameof(EView))
                    .RegisterLoadingIndicator(BuildLoadingIndicator)
                    .RegisterErrorIndicator(BuildErrorIndicator)
                    .RegisterView<ListBoxRegionView, ListBoxRegionViewModel>(nameof(ListBoxRegionView));

            var sp = services.BuildServiceProvider();
            base.OnStartup(e);

            var mainWindow = new MainWindow
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            };
            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        private FrameworkElement BuildLoadingIndicator(IServiceProvider sp, NavigationContext navigationContext)
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

        private FrameworkElement BuildErrorIndicator(IServiceProvider sp, NavigationContext navigationContext)
        {
            var textFailed = new TextBlock
            {
                Text = "Failed",
                FontSize = 20,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            DockPanel.SetDock(textFailed, Dock.Top);
            var error = new TextBlock
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
}
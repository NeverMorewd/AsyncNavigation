using AsyncNavigation;
using AsyncNavigation.Abstractions;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sample.Common;

namespace Sample.FrontDialog;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public async override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddNavigationSupport()
                .AddSingleton<MainWindowViewModel>()
                .RegisterDialogWindow<DialogWindow>()
                .RegisterView<FrontView, FrontDialogViewModel>(nameof(FrontView));
        var sp = services.BuildServiceProvider();

        var dialogService = sp.GetRequiredService<IDialogService>();
        await dialogService.FrontShowAsync(nameof(FrontView), result => 
        {
            var win = new MainWindow
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            };
            return win;
        },
        nameof(DialogWindow));
        base.OnFrameworkInitializationCompleted();
    }
}
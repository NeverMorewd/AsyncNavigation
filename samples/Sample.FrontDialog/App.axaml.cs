using AsyncNavigation.Abstractions;
using AsyncNavigation;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sample.Common;
using System;
using System.Diagnostics;

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
                .AddSingletonWitAllMembers<MainWindowViewModel>()
                .RegisterDialogWindow<DialogWindow, FrontDialogViewModel>("DialogWindow");
        
        var sp = services.BuildServiceProvider();

        var dialogService = sp.GetRequiredService<IDialogService>();
        try
        {
            await dialogService.FrontShowWindowAsync("DialogWindow", result =>
            {
                if (result.Result == AsyncNavigation.Core.DialogButtonResult.Done)
                {
                    var win = new MainWindow
                    {
                        DataContext = sp.GetRequiredService<MainWindowViewModel>()
                    };
                    return win;
                }
                else
                {
                    if (Current?.ApplicationLifetime is IControlledApplicationLifetime applicationLifetime)
                    {
                        applicationLifetime.Shutdown();
                    }
                    return null;
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        base.OnFrameworkInitializationCompleted();
    }
}
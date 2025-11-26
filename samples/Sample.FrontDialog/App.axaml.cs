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

    public override async void OnFrameworkInitializationCompleted()
    {
        try
        {
            var services = new ServiceCollection();
#pragma warning disable IL2026
            services.AddNavigationSupport()
                .AddSingletonWitAllMembers<MainWindowViewModel>()
#pragma warning restore IL2026
                .RegisterDialogWindow<DialogWindow, FrontDialogViewModel>("DialogWindow");
        
            var sp = services.BuildServiceProvider();

            var dialogService = sp.GetRequiredService<IDialogService>();
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
            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}
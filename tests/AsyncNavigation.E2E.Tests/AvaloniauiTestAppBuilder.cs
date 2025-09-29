using AsyncNavigation.E2E.Tests;
using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(AvaloniauiTestAppBuilder))]


namespace AsyncNavigation.E2E.Tests;

public class AvaloniauiTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
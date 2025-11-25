using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using ReactiveUI.Avalonia;

namespace Sample.Avalonia.Android
{
    [Activity(
        Label = "Sample.Avalonia.Android",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App>
    {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            var abuilder = base.CustomizeAppBuilder(builder)
                .WithInterFont()
                .UseReactiveUI();
            var app = abuilder.Instance as App;
            return abuilder;
        }
    }
}

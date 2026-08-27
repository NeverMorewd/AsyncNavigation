using AsyncNavigation.WinUI;

namespace Sample.WinUI.Views;

public sealed class AWindow : DefaultDialogContainer
{
    public AWindow()
    {
        Title = nameof(AWindow);
        Content = new LightView();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(800, 450));
    }
}

using AsyncNavigation.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Sample.Common;

namespace Sample.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is MainWindowViewModel mainWindowViewModel)
        {
            SearchBox.ItemFilter = MainWindowViewModel.FilterPredicate;
            footer.Text = $"{mainWindowViewModel.FooterText} - Avalonia:{typeof(Application).Assembly.GetName().Version}";
        }
    }
}
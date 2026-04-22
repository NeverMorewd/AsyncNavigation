using AsyncNavigation.Abstractions;
using Avalonia;
using Avalonia.Controls;

namespace Sample.Avalonia.Views;

public partial class ItemsRegionView : UserControl, IView
{
    public ItemsRegionView()
    {
        InitializeComponent();
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }
}
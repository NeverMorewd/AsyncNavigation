using AsyncNavigation.Abstractions;
using Avalonia;
using Avalonia.Controls;

namespace Sample.Avalonia.Views;

public partial class BView : UserControl, IView
{
    public BView()
    {
        InitializeComponent();
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }
}
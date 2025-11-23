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
        // SearchBox.Bind(
        //     AutoCompleteBox.ItemFilterProperty,
        //     new Binding(nameof(ItemFilter)) { Source = this, Mode = BindingMode.TwoWay });
        //SearchBox.ItemFilter = ItemFilter;
        if (DataContext is MainWindowViewModel mainWindowViewModel)
        {
            SearchBox.ItemFilter = MainWindowViewModel.FilterPredicate;
        }
    }
}
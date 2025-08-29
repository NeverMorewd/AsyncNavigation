using AsyncNavigation.Abstractions;
using System.Windows;

namespace AsyncNavigation.Wpf;

/// <summary>
/// Interaction logic for DefaultDialogContainer.xaml
/// </summary>
public partial class DefaultDialogContainer : Window, IDialogWindowBase
{
    public DefaultDialogContainer()
    {
        InitializeComponent();
    }
}

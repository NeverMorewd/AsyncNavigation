using AsyncNavigation.Abstractions;
using System.Windows;

namespace AsyncNavigation.WinUI;

/// <summary>
/// Interaction logic for DefaultDialogContainer.xaml
/// </summary>
public partial class DefaultDialogContainer : Window, IDialogWindow
{
    public DefaultDialogContainer()
    {
        InitializeComponent();
    }
}

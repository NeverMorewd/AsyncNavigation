using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public partial class DefaultDialogContainer : Window, IDialogWindow
{
    public DefaultDialogContainer()
    {
        InitializeComponent();
    }
}
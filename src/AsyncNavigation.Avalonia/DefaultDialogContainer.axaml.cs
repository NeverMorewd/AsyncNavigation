using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public partial class DefaultDialogContainer : Window, IWindowBase
{
    public DefaultDialogContainer()
    {
        InitializeComponent();
    }
}
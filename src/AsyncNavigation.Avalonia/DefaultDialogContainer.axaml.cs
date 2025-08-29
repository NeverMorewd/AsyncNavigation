using AsyncNavigation.Abstractions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

namespace AsyncNavigation.Avalonia;

public partial class DefaultDialogContainer : Window, IDialogWindowBase
{
    public DefaultDialogContainer()
    {
        InitializeComponent();
    }
}
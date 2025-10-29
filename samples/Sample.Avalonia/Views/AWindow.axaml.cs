using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace Sample.Avalonia.Views;

public partial class AWindow : Window, IDialogWindow
{
    public AWindow()
    {
        InitializeComponent();
    }
}
using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace Sample.FrontDialog;

public partial class DialogWindow : Window, IDialogWindow
{
    public DialogWindow()
    {
        InitializeComponent();
    }
}
using AsyncNavigation.Abstractions;
using Microsoft.UI.Xaml;
using System;

namespace AsyncNavigation.WinUI;

/// <summary>
/// Interaction logic for DefaultDialogContainer.xaml
/// </summary>
public partial class DefaultDialogContainer : Window, IDialogWindow
{
    public DefaultDialogContainer()
    {
        InitializeComponent();
        Closed += (_, _) => DialogClosed?.Invoke(this, EventArgs.Empty);
    }

    object? IDialogWindowBase.Content
    {
        get => Content;
        set => Content = value as UIElement
            ?? throw new ArgumentException("Dialog content must be a WinUI UIElement.", nameof(value));
    }

    object? IDialogWindowBase.DataContext
    {
        get => (Content as FrameworkElement)?.DataContext;
        set
        {
            if (Content is FrameworkElement element)
                element.DataContext = value;
        }
    }

    event EventHandler? IDialogWindowBase.Closed
    {
        add => DialogClosed += value;
        remove => DialogClosed -= value;
    }

    void IDialogWindowBase.Show() => Activate();

    private event EventHandler? DialogClosed;
}

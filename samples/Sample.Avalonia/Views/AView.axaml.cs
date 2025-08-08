using AsyncNavigation.Abstractions;
using Avalonia.Controls;
using System;
using System.Threading;

namespace Sample.Avalonia.Views;

public partial class AView : UserControl, IView
{
    public AView()
    {
        InitializeComponent();
    }
}
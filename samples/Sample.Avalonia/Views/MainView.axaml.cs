using AsyncNavigation.Core;
using AsyncNavigation.Floating;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Sample.Common;
using System;
using System.Diagnostics;

namespace Sample.Avalonia.Views;

public partial class MainView : UserControl
{
    private readonly IViewPlacementService? _placementService;

    public MainView() : this(null)
    {
    }

    public MainView(IViewPlacementService? placementService)
    {
        _placementService = placementService;
        InitializeComponent();
    }

    private async void FloatMainRegion_Click(object? sender, RoutedEventArgs e)
    {
        if (_placementService is null)
            return;

        try
        {
            await _placementService.FloatAsync("MainRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating MainRegion",
                Width = 900,
                Height = 600
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            Debug.WriteLine($"Cannot float MainRegion: {ex.Message}");
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is MainWindowViewModel mainWindowViewModel)
        {
            SearchBox.ItemFilter = MainWindowViewModel.FilterPredicate;
            footer.Text = $"{mainWindowViewModel.FooterText} - Avalonia:{typeof(Application).Assembly.GetName().Version}";
        }
    }
}
using AsyncNavigation.Abstractions;
using AsyncNavigation.Floating;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace Sample.Avalonia.Views;

public partial class ItemsRegionView : UserControl, IView
{
    private readonly IViewPlacementService? _placementService;

    public ItemsRegionView() : this(null)
    {
    }

    public ItemsRegionView(IViewPlacementService? placementService)
    {
        _placementService = placementService;
        InitializeComponent();
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    private async void FloatSelectedItem_Click(object? sender, RoutedEventArgs e)
    {
        if (_placementService is null)
            return;

        try
        {
            await _placementService.FloatAsync("ItemsRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating ItemsRegion item",
                Width = 720,
                Height = 480
            });
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"Cannot float ItemsRegion: {ex.Message}");
        }
    }
}

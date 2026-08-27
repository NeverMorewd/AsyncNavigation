using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncNavigation.WinUI;

public sealed class NavigationViewRegion : RegionBase<NavigationViewRegion, NavigationView>
{
    private readonly NavigationView _navigationView;
    private readonly IRegionManager _regionManager;
    private string? _activeViewName;

    public NavigationViewRegion(
        string name,
        NavigationView navigationView,
        IServiceProvider serviceProvider,
        bool? useCache)
        : base(name, navigationView, serviceProvider)
    {
        _navigationView = navigationView;
        _regionManager = serviceProvider.GetRequiredService<IRegionManager>();
        EnableViewCache = useCache ?? true;
        IsSinglePageRegion = true;

        _navigationView.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        _navigationView.VerticalContentAlignment = VerticalAlignment.Stretch;
        _navigationView.ItemInvoked += OnItemInvoked;
        _navigationView.BackRequested += OnBackRequested;
        Navigated += OnNavigated;
    }

    public override NavigationPipelineMode NavigationPipelineMode => NavigationPipelineMode.RenderFirst;

    public override async Task ProcessActivateAsync(NavigationContext navigationContext)
    {
        _activeViewName = navigationContext.ViewName;
        _navigationView.Content = navigationContext.IndicatorHost.Value?.Host;
        _navigationView.Header = navigationContext.ViewName;

        var matchingItem = FindMenuItem(navigationContext.ViewName);
        if (matchingItem is not null)
            _navigationView.SelectedItem = matchingItem;

        await UpdateBackButtonAsync();
    }

    public override async Task ProcessDeactivateAsync(NavigationContext? navigationContext)
    {
        _activeViewName = null;
        _navigationView.Content = null;
        await UpdateBackButtonAsync();
    }

    public override void Dispose()
    {
        _navigationView.ItemInvoked -= OnItemInvoked;
        _navigationView.BackRequested -= OnBackRequested;
        Navigated -= OnNavigated;
        _navigationView.Content = null;
        base.Dispose();
    }

    private async void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked || args.InvokedItemContainer?.Tag is not string viewName || string.IsNullOrWhiteSpace(viewName))
            return;
        if (string.Equals(_activeViewName, viewName, StringComparison.Ordinal))
            return;

        try
        {
            await _regionManager.RequestNavigateAsync(Name, viewName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"NavigationView navigation to '{viewName}' failed: {ex}");
        }
    }

    private async void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        try
        {
            if (await ((IRegion)this).CanGoBackAsync())
                await GoBackAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"NavigationView back navigation failed: {ex}");
        }
        finally
        {
            await UpdateBackButtonAsync();
        }
    }

    private async Task UpdateBackButtonAsync()
        => _navigationView.IsBackEnabled = await ((IRegion)this).CanGoBackAsync();

    private async void OnNavigated(object? sender, NavigationEventArgs e)
        => await UpdateBackButtonAsync();

    private NavigationViewItem? FindMenuItem(string viewName)
        => FindMenuItem(_navigationView.MenuItems.Concat(_navigationView.FooterMenuItems), viewName);

    private static NavigationViewItem? FindMenuItem(IEnumerable<object> items, string viewName)
    {
        foreach (var item in items.OfType<NavigationViewItem>())
        {
            if (string.Equals(item.Tag as string, viewName, StringComparison.Ordinal))
                return item;

            var nestedItem = FindMenuItem(item.MenuItems, viewName);
            if (nestedItem is not null)
                return nestedItem;
        }

        return null;
    }
}

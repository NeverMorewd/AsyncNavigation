using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI.SourceGenerators;
using Sample.Common.Messages;
using System.Diagnostics;

namespace Sample.Common;

public partial class AViewModel : InstanceCounterViewModel<AViewModel>, IDialogAware, IViewAware
{
    private readonly IRegionManager _regionManager;
    private readonly IMessenger _messenger;

    // Stored by OnViewAttached; cleared by OnViewDetached.
    // Platform-specific features are accessed via extension methods:
    //   Avalonia: _viewContext.GetStorageProvider(), .GetClipboard(), .GetTopLevel()  → using AsyncNavigation.Avalonia;
    //   WPF:      _viewContext.GetWindow(), .GetDispatcher()                          → using AsyncNavigation.Wpf;
    private IViewContext? _viewContext;

    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    public string Title => $"{nameof(AViewModel)}:{InstanceNumber}";

    public AViewModel(IRegionManager regionManager, IMessenger messenger)
    {
        _regionManager = regionManager;
        _messenger = messenger;
    }

    // ── IViewAware ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by the navigation framework after the view is attached to the visual tree.
    /// Platform-specific services (StorageProvider, Window, …) are available here.
    /// </summary>
    public void OnViewAttached(IViewContext context)
    {
        _viewContext = context;

        // Platform context is available. In a project that also references
        // AsyncNavigation.Avalonia / AsyncNavigation.Wpf you can write:
        //
        //   var sp = context.GetStorageProvider();   // Avalonia
        //   var win = context.GetWindow();           // WPF

        Debug.WriteLine($"[{Title}] OnViewAttached — platform context: {context.GetType().Name}");

        // Notify any interested party (e.g. BViewModel) without a direct reference.
        _messenger.Send(new ViewAttachedMessage(Title, context.GetType().Name));
    }

    /// <summary>Called when the view is removed from the visual tree (single-page region only).</summary>
    public void OnViewDetached()
    {
        _viewContext = null;
        Debug.WriteLine($"[{Title}] OnViewDetached.");
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        return RequestUnloadAsync(CancellationToken.None);
    }

    [ReactiveCommand]
    private Task CloseDialog(string param)
    {
        return RequestCloseAsync!.Invoke(this,
            new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK),
            CancellationToken.None));
    }

    [ReactiveCommand]
    private Task CloseDialogWithCancelling(string param)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));
        return RequestCloseAsync!.Invoke(this, new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), cts.Token));
    }

    // ── IDialogAware ─────────────────────────────────────────────────────────

    public async Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        IsDialog = true;
        if (cancellationToken.CanBeCanceled)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
        _messenger.Send(new ViewAttachedMessage(Title, nameof(OnDialogOpenedAsync)));
    }

    public async Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        if (cancellationToken.CanBeCanceled)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

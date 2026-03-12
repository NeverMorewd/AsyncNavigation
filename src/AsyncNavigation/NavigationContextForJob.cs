using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation;

public partial class NavigationContext : IJobContext
{
    private CancellationTokenSource? _cts = null;
    // Intermediate linked CTS objects created by LinkCancellationToken().
    // They must stay alive (not disposed) while navigation is in progress so that
    // cancellation can still propagate through the chain.  All are disposed together
    // in OnNavigationCompleted().
    private List<CancellationTokenSource>? _linkedCtsList;
    private readonly object _ctsLock = new();
    Guid IJobContext.JobId
    {
        get => NavigationId;
    }
    void IJobContext.OnStarted()
    {
        UpdateStatus(NavigationStatus.InProgress);
    }

    void IJobContext.OnCompleted()
    {
        OnNavigationCompleted();
    }

    public void LinkCancellationToken(CancellationToken otherToken)
    {
        if (!otherToken.CanBeCanceled)
            return;

        var oldCts = _cts;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, otherToken);
        CancellationToken = _cts.Token;

        // Do NOT dispose oldCts here.  _cts was created by linking against oldCts.Token,
        // so disposing oldCts would unregister its callback on the upstream token and break
        // the cancellation chain.  Defer disposal to OnNavigationCompleted().
        if (oldCts is not null)
        {
            _linkedCtsList ??= [];
            _linkedCtsList.Add(oldCts);
        }
    }

    public async Task<bool> CancelAndWaitAsync(TimeSpan? timeout = null)
    {
        lock (_ctsLock)
        {
            _cts ??= new CancellationTokenSource();
            CancellationToken = _cts.Token;
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }

        if (IsCompleted)
        {
            return true;
        }

        try
        {
            if (timeout.HasValue)
            {
                using var timeoutCts = new CancellationTokenSource(timeout.Value);
                await _completionTcs.Task.WaitAsync(timeoutCts.Token);
            }
            else
            {
                await _completionTcs.Task;
            }
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}

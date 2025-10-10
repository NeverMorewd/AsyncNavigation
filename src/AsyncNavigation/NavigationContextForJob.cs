using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation;

public partial class NavigationContext : IJobContext
{
    private CancellationTokenSource? _cts = null;
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

        lock (_ctsLock)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, otherToken);
            CancellationToken = _cts.Token;
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

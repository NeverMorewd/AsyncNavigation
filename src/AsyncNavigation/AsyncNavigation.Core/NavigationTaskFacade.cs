using System.Runtime.CompilerServices;

namespace AsyncNavigation.Core;

internal class NavigationTaskFacade
{
    private readonly NavigationContext _context;
    private readonly Task _precedingTask;
    private readonly Task _resolveViewTask;
    private readonly Task _remainingTask;
    internal NavigationTaskFacade(Task precedingTask,
        Task resolveViewTask,
        Task remainingTask,
        NavigationContext context)
    {
        _precedingTask = precedingTask;
        _resolveViewTask = resolveViewTask;
        _remainingTask = remainingTask;
        _context = context;
    }

    public TaskAwaiter GetAwaiter()
    {
        return WaitDefault().GetAwaiter();
    }
    public Task WaitDefault()
    {
        ThrowIfHasError();
        return Task.WhenAll(_precedingTask, _resolveViewTask, _remainingTask);
    }
    public async Task<object> WaitResolveViewTaskAsync()
    {
        ThrowIfHasError();
        await _precedingTask;
        await _resolveViewTask;
        return _context.Target.Value!;
    }
    private void ThrowIfHasError()
    {
        if (_context.Errors != null && _context.Errors.Count != 0)
        {
            throw new AggregateException(_context.Errors);
        }
    }
}

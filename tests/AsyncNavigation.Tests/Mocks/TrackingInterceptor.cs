using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

public class TrackingInterceptor : INavigationInterceptor
{
    public List<NavigationContext> NavigatingContexts { get; } = [];
    public List<NavigationContext> NavigatedContexts { get; } = [];
    public bool ThrowOnNavigating { get; set; }
    public bool ThrowOnNavigated { get; set; }

    public void Reset()
    {
        NavigatingContexts.Clear();
        NavigatedContexts.Clear();
        ThrowOnNavigating = false;
        ThrowOnNavigated = false;
    }

    public Task OnNavigatingAsync(NavigationContext context)
    {
        NavigatingContexts.Add(context);
        if (ThrowOnNavigating)
            throw new OperationCanceledException("Interceptor cancelled navigation.");
        return Task.CompletedTask;
    }

    public Task OnNavigatedAsync(NavigationContext context)
    {
        NavigatedContexts.Add(context);
        if (ThrowOnNavigated)
            throw new InvalidOperationException("Interceptor error.");
        return Task.CompletedTask;
    }
}

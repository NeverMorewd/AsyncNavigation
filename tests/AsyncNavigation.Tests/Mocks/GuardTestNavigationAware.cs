using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class GuardTestNavigationAware : INavigationAware, INavigationGuard
{
    public static GuardTestNavigationAware? LastCreated { get; private set; }

    /// <summary>Set before navigating (no view caching means a fresh instance is created per
    /// navigation, so a static hook is what lets the next-created instance(s) pick it up) to run
    /// <see cref="OnActivatedCallback"/> on whichever instance gets created next. Not consumed/
    /// cleared on construction: GuardTestView's own constructor parameter causes the DI container
    /// to construct a throwaway instance before the one actually wired as its DataContext, so
    /// every instance built while this is set picks up the same reference - only the one that
    /// ends up as the real DataContext ever has OnNavigatedToAsync invoked on it. Callers should
    /// reset this back to null themselves once done to avoid leaking into other tests.</summary>
    public static Action? NextActivatedCallback { get; set; }

    public GuardTestNavigationAware()
    {
        LastCreated = this;
        OnActivatedCallback = NextActivatedCallback;
    }

    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
    public bool AllowNavigation { get; set; } = true;
    public bool GuardWasCalled { get; private set; }
    public bool NavigatedFromWasCalled { get; private set; }

    /// <summary>Invoked at the end of <see cref="OnNavigatedToAsync"/>, before it returns - lets a
    /// test simulate cancellation racing with a just-completed activation.</summary>
    public Action? OnActivatedCallback { get; set; }

    public Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken cancellationToken)
    {
        GuardWasCalled = true;
        return Task.FromResult(AllowNavigation);
    }

    public Task InitializeAsync(NavigationContext context) => Task.CompletedTask;
    public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(true);
    public Task OnNavigatedFromAsync(NavigationContext context) { NavigatedFromWasCalled = true; return Task.CompletedTask; }
    public Task OnNavigatedToAsync(NavigationContext context)
    {
        OnActivatedCallback?.Invoke();
        return Task.CompletedTask;
    }
    public Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

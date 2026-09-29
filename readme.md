# AsyncNavigation

> A lightweight async navigation framework for .NET desktop apps, built on `Microsoft.Extensions.DependencyInjection`.

[![CI](https://github.com/NeverMorewd/AsyncNavigation/actions/workflows/ci.yml/badge.svg)](https://github.com/NeverMorewd/AsyncNavigation/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.svg?label=Core&color=004880)](https://www.nuget.org/packages/AsyncNavigation)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.Avalonia.svg?label=Avalonia&color=8b45e0)](https://www.nuget.org/packages/AsyncNavigation.Avalonia)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.Wpf.svg?label=WPF&color=0078d4)](https://www.nuget.org/packages/AsyncNavigation.Wpf)
[![WinUI 3](https://img.shields.io/badge/WinUI%203-in%20development-orange)](samples/Sample.WinUI)
[![License: MIT](https://img.shields.io/github/license/NeverMorewd/AsyncNavigation)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4)](https://dotnet.microsoft.com)

**[中文文档](readme_zh-cn.md)** · **[Live Demo](https://nevermorewd.github.io/AsyncNavigation/)**

---

## Features

| | |
|---|---|
| **Async/Await Native** | Navigation is fully async end-to-end with `CancellationToken` support |
| **DI-First** | Views and view models are resolved from the DI container |
| **Navigation Guard** | Block navigation away with `INavigationGuard` (e.g. unsaved changes) |
| **Interceptors** | Run cross-cutting logic (auth, analytics) via `INavigationInterceptor` |
| **Dialog Service** | Async dialog and window management built-in |
| **Multiple Region Types** | `ContentControl`, `ItemsControl`, and `TabControl` regions |
| **History Navigation** | `GoForwardAsync` / `GoBackAsync` out of the box |
| **Lifecycle Management** | Automatic view caching, eviction, and disposal — no memory leaks |
| **Native AOT** | Full Avalonia AOT / trimming support, zero extra config |
| **Framework-Agnostic** | Works with any MVVM framework |
| **Minimal Deps** | Only `Microsoft.Extensions.DependencyInjection.Abstractions >= 8.0` |

---

## Platform Support

| Platform | Status | Package / Sample |
|---|---|---|
| Avalonia | Stable | `AsyncNavigation.Avalonia` |
| WPF | Stable | `AsyncNavigation.Wpf` |
| WinUI 3 | **In development** | [`Sample.WinUI`](samples/Sample.WinUI) |

> [!WARNING]
> WinUI 3 support is under active development. It includes content, items, tab, `NavigationView`, dialog, window, and indicator support, but its APIs and behavior may still change. Test it carefully before using it in production.

---

## Installation

```bash
# Avalonia
dotnet add package AsyncNavigation.Avalonia

# WPF
dotnet add package AsyncNavigation.Wpf
```

WinUI 3 is currently available from source and through the repository sample while development continues.

---

## Quick Start

### 1. Register services

```csharp
services.AddNavigationSupport()
        .RegisterView<HomeView, HomeViewModel>("Home")
        .RegisterView<SettingsView, SettingsViewModel>("Settings")
        .RegisterDialog<ConfirmView, ConfirmViewModel>("Confirm");
```

### 2. Declare a region in XAML

```xml
xmlns:an="https://github.com/NeverMorewd/AsyncNavigation"

<ContentControl an:RegionManager.RegionName="MainRegion" />
```

### 3. Navigate

```csharp
// Navigate
await _regionManager.RequestNavigateAsync("MainRegion", "Home");

// History
await _regionManager.GoBackAsync("MainRegion");
await _regionManager.GoForwardAsync("MainRegion");

// Dialog
var result = await _dialogService.ShowViewDialogAsync("Confirm");
```

### 4. React to navigation in view models

View models implement `INavigationAware` directly. `NavigationAwareBase` remains available for compatibility but is obsolete; new code should implement `INavigationAware`. You can define an application base class to share default implementations.

```csharp
using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Threading;
using System.Threading.Tasks;

public class HomeViewModel : INavigationAware
{
    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;

    public Task InitializeAsync(NavigationContext context) => Task.CompletedTask;

    public Task OnNavigatedToAsync(NavigationContext context)
    {
        // Load page data here; use context.CancellationToken for async operations.
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Allow reuse when caching is enabled; return false to request a new instance.
    public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(true);
}
```

---

## Navigation Guard

```csharp
public class EditViewModel : INavigationAware, INavigationGuard
{
    public bool HasUnsavedChanges { get; set; }
    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;

    public Task InitializeAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnNavigatedToAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnNavigatedFromAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(true);

    public Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken ct)
    {
        // Return false to cancel; show a confirmation dialog asynchronously here if needed.
        return Task.FromResult(!HasUnsavedChanges);
    }
}
```

## Interceptors

```csharp
public class AuthInterceptor : INavigationInterceptor
{
    public Task OnNavigatingAsync(NavigationContext context)
    {
        if (!_auth.IsLoggedIn)
            throw new OperationCanceledException("Not authenticated.");
        return Task.CompletedTask;
    }

    public Task OnNavigatedAsync(NavigationContext context) => Task.CompletedTask;
}

// Register
services.AddNavigationSupport()
        .RegisterNavigationInterceptor<AuthInterceptor>();
```

---

## License

MIT

# AsyncNavigation

> A lightweight async navigation framework for .NET desktop apps, built on `Microsoft.Extensions.DependencyInjection`.

[![CI](https://github.com/NeverMorewd/AsyncNavigation/actions/workflows/ci.yml/badge.svg)](https://github.com/NeverMorewd/AsyncNavigation/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.svg?label=Core&color=004880)](https://www.nuget.org/packages/AsyncNavigation)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.Avalonia.svg?label=Avalonia&color=8b45e0)](https://www.nuget.org/packages/AsyncNavigation.Avalonia)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.Wpf.svg?label=WPF&color=0078d4)](https://www.nuget.org/packages/AsyncNavigation.Wpf)
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

## Installation

```bash
# Avalonia
dotnet add package AsyncNavigation.Avalonia

# WPF
dotnet add package AsyncNavigation.Wpf
```

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

```csharp
// Derive from NavigationAwareBase and override only what you need.
public class HomeViewModel : NavigationAwareBase
{
    public override async Task OnNavigatedToAsync(NavigationContext context)
    {
        await LoadDataAsync(context.CancellationToken);
    }
}
```

---

## Navigation Guard

```csharp
public class EditViewModel : NavigationAwareBase, INavigationGuard
{
    public async Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken ct)
    {
        // Return false to cancel — show a confirmation dialog here if needed.
        return !HasUnsavedChanges;
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

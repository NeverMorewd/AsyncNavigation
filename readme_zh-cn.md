# AsyncNavigation

> 基于 `Microsoft.Extensions.DependencyInjection` 的轻量级 .NET 桌面应用异步导航框架。

[![CI](https://github.com/NeverMorewd/AsyncNavigation/actions/workflows/ci.yml/badge.svg)](https://github.com/NeverMorewd/AsyncNavigation/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.svg?label=Core&color=004880)](https://www.nuget.org/packages/AsyncNavigation)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.Avalonia.svg?label=Avalonia&color=8b45e0)](https://www.nuget.org/packages/AsyncNavigation.Avalonia)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.Wpf.svg?label=WPF&color=0078d4)](https://www.nuget.org/packages/AsyncNavigation.Wpf)
[![WinUI 3](https://img.shields.io/badge/WinUI%203-%E5%BC%80%E5%8F%91%E4%B8%AD-orange)](samples/Sample.WinUI)
[![License: MIT](https://img.shields.io/github/license/NeverMorewd/AsyncNavigation)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4)](https://dotnet.microsoft.com)

**[English](readme.md)** · **[在线演示](https://nevermorewd.github.io/AsyncNavigation/)**

---

## 功能特性

| | |
|---|---|
| **原生异步** | 全链路 `async/await`，内置 `CancellationToken` 支持 |
| **DI 优先** | 视图与视图模型均由 DI 容器解析 |
| **导航守卫** | 通过 `INavigationGuard` 阻断导航（如未保存提示） |
| **导航拦截器** | 通过 `INavigationInterceptor` 实现鉴权、埋点等横切逻辑 |
| **对话框服务** | 内置异步对话框与窗口管理 |
| **多种 Region 类型** | 支持 `ContentControl`、`ItemsControl`、`TabControl` |
| **历史导航** | 开箱即用的 `GoForwardAsync` / `GoBackAsync` |
| **生命周期管理** | 自动处理视图缓存、淘汰与释放，防止内存泄漏 |
| **Native AOT** | 完整支持 Avalonia AOT 编译与裁剪，无需额外配置 |
| **框架无关** | 可与任意 MVVM 框架配合使用 |
| **依赖极少** | 仅依赖 `Microsoft.Extensions.DependencyInjection.Abstractions >= 8.0` |

---

## 平台支持

| 平台 | 状态 | 包 / 示例 |
|---|---|---|
| Avalonia | 稳定 | `AsyncNavigation.Avalonia` |
| WPF | 稳定 | `AsyncNavigation.Wpf` |
| WinUI 3 | **开发中** | [`Sample.WinUI`](samples/Sample.WinUI) |

> [!WARNING]
> WinUI 3 支持仍处于积极开发阶段。目前已包含 Content、Items、Tab、`NavigationView`、对话框、窗口和加载指示器支持，但 API 与行为仍可能发生变化。在生产环境使用前请充分测试。

---

## 安装

```bash
# Avalonia
dotnet add package AsyncNavigation.Avalonia

# WPF
dotnet add package AsyncNavigation.Wpf
```

WinUI 3 目前可通过源码和仓库中的示例项目进行体验，正式包仍在开发中。

---

## 快速开始

### 1. 注册服务

```csharp
services.AddNavigationSupport()
        .RegisterView<HomeView, HomeViewModel>("Home")
        .RegisterView<SettingsView, SettingsViewModel>("Settings")
        .RegisterDialog<ConfirmView, ConfirmViewModel>("Confirm");
```

### 2. 在 XAML 中声明 Region

```xml
xmlns:an="https://github.com/NeverMorewd/AsyncNavigation"

<ContentControl an:RegionManager.RegionName="MainRegion" />
```

### 3. 执行导航

```csharp
// 页面导航
await _regionManager.RequestNavigateAsync("MainRegion", "Home");

// 历史记录
await _regionManager.GoBackAsync("MainRegion");
await _regionManager.GoForwardAsync("MainRegion");

// 对话框
var result = await _dialogService.ShowViewDialogAsync("Confirm");
```

### 4. 在视图模型中响应导航

视图模型直接实现 `INavigationAware`。库不提供导航基类；如需复用默认实现，可以在应用中定义自己的基类。

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
        // 在此加载页面数据；异步操作可使用 context.CancellationToken。
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // 启用缓存时，允许复用此实例；返回 false 可请求新实例。
    public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(true);
}
```

---

## 导航守卫

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
        // 返回 false 可取消导航；如需确认，可在此异步显示对话框。
        return Task.FromResult(!HasUnsavedChanges);
    }
}
```

## 导航拦截器

```csharp
public class AuthInterceptor : INavigationInterceptor
{
    public Task OnNavigatingAsync(NavigationContext context)
    {
        if (!_auth.IsLoggedIn)
            throw new OperationCanceledException("未登录");
        return Task.CompletedTask;
    }

    public Task OnNavigatedAsync(NavigationContext context) => Task.CompletedTask;
}

// 注册
services.AddNavigationSupport()
        .RegisterNavigationInterceptor<AuthInterceptor>();
```

---

## 许可证

MIT

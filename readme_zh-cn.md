# AsyncNavigation

> 基于 `Microsoft.Extensions.DependencyInjection` 的轻量级 .NET 桌面应用异步导航框架。

[![CI](https://github.com/NeverMorewd/AsyncNavigation/actions/workflows/ci.yml/badge.svg)](https://github.com/NeverMorewd/AsyncNavigation/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.svg?label=Core&color=004880)](https://www.nuget.org/packages/AsyncNavigation)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.Avalonia.svg?label=Avalonia&color=8b45e0)](https://www.nuget.org/packages/AsyncNavigation.Avalonia)
[![NuGet](https://img.shields.io/nuget/v/AsyncNavigation.Wpf.svg?label=WPF&color=0078d4)](https://www.nuget.org/packages/AsyncNavigation.Wpf)
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

## 安装

```bash
# Avalonia
dotnet add package AsyncNavigation.Avalonia

# WPF
dotnet add package AsyncNavigation.Wpf
```

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

```csharp
// 继承 NavigationAwareBase，按需重写方法即可。
public class HomeViewModel : NavigationAwareBase
{
    public override async Task OnNavigatedToAsync(NavigationContext context)
    {
        await LoadDataAsync(context.CancellationToken);
    }
}
```

---

## 导航守卫

```csharp
public class EditViewModel : NavigationAwareBase, INavigationGuard
{
    public async Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken ct)
    {
        // 返回 false 可取消导航，通常在此弹出确认对话框
        return !HasUnsavedChanges;
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

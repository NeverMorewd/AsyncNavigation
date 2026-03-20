# 🚀 AsyncNavigation

[中文文档](readme_zh-cn.md)


> A lightweight asynchronous navigation framework based on **Microsoft.Extensions.DependencyInjection**

Try it online : [demo](https://nevermorewd.github.io/AsyncNavigation/)
---

## ✨ Features

-  **Fully Asynchronous Navigation**  
  Natively supports `async/await`, making navigation and asynchronous operations seamless and intuitive.

-  **Built-in DialogService**  
  Provides asynchronous implementation for dialog navigation.

-  **Supports Cancellation**  
  Built-in `CancellationToken` support allows safe cancellation of navigation at any time.

-  **Customizable Navigation Indicators**  
  Developers can define custom indicators to visualize navigation states such as *loading*, *error*, or *completed*.

-  **Multiple Built-in Region Types**  
  In addition to standard single-page navigation based on `ContentControl`, also supports `ItemsControl` and `TabControl`.

-  **Minimal Dependencies**  
  Depends only on `Microsoft.Extensions.DependencyInjection.Abstractions (>= 8.0)`.

-  **Framework-Agnostic**  
  Works with any MVVM framework, giving developers complete freedom of choice.

-  **Support Native Aot**  
  Fully supports Avalonia's Native AOT compilation and trimming, without any additional configuration required.

-  **RegionAdapter Extension Mechanism**  
  Extend and customize navigation behaviors by implementing your own `RegionAdapter`.

-  **Fine-grained Control Options**  
  Offers rich configuration options to make navigation behavior align precisely with application needs.

-  **Lifecycle Management**
  Automatically handles view creation, caching, and release — effectively preventing memory leaks.

-  **NavigationAwareBase Convenience Class**
  Derive from `NavigationAwareBase` instead of implementing all `INavigationAware` members manually — override only what you need.

-  **Navigation Guard**
  Implement `INavigationGuard` on any view model to block navigation away (e.g., unsaved-changes confirmation).

-  **Navigation Interceptors**
  Register `INavigationInterceptor` implementations to run cross-cutting logic (authentication, analytics, redirects) for every navigation request.

-  **Highly Abstract Core Layer**  
  Core logic is encapsulated within abstractions, minimizing platform-specific code and improving testability.


---

## 📦 Installation

### WPF
```bash
dotnet add package AsyncNavigation.Wpf
```

### Avaloniaui
```bash
dotnet add package AsyncNavigation.Avaloniaui
```

## ⚡ Get started

#### Region

  ###### Set Namespace
```
 xmlns:an="https://github.com/NeverMorewd/AsyncNavigation"
```
  ###### Set RegionName
```xml
 <ContentControl an:RegionManager.RegionName="MainRegion" />
```

#### ViewModel
<details>
<summary>Code Examples</summary>

```csharp

// Derive from NavigationAwareBase and override only what you need.
// All methods have no-op defaults so you never have to write boilerplate.
public class SampleViewModel : NavigationAwareBase
{
    public override async Task OnNavigatedToAsync(NavigationContext context)
    {
        await LoadDataAsync(context.CancellationToken);
    }

    public override async Task OnNavigatedFromAsync(NavigationContext context)
    {
        await SaveStateAsync(context.CancellationToken);
    }
}

```

</details>

#### Navigation Guard
<details>
<summary>Code Examples</summary>

Implement `INavigationGuard` alongside `NavigationAwareBase` (or `INavigationAware`) to block navigation away from the current view — for example, when the user has unsaved changes.

```csharp

public class EditViewModel : NavigationAwareBase, INavigationGuard
{
    public bool HasUnsavedChanges { get; private set; }

    public async Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken cancellationToken)
    {
        if (!HasUnsavedChanges)
            return true;

        // Return false to cancel the navigation.
        // In a real app you might show a confirmation dialog here.
        return false;
    }
}

```

</details>

#### Navigation Interceptor
<details>
<summary>Code Examples</summary>

Implement `INavigationInterceptor` to run cross-cutting logic for every navigation request (e.g., authentication, analytics, global redirects).

```csharp

public class AuthInterceptor : INavigationInterceptor
{
    private readonly IAuthService _auth;

    public AuthInterceptor(IAuthService auth) => _auth = auth;

    public Task OnNavigatingAsync(NavigationContext context)
    {
        if (!_auth.IsLoggedIn)
            throw new OperationCanceledException("Not authenticated.");
        return Task.CompletedTask;
    }

    public Task OnNavigatedAsync(NavigationContext context) => Task.CompletedTask;
}

```

Register via the DI extension:

```csharp

services.AddNavigationSupport()
        .RegisterView<HomeView, HomeViewModel>("Home")
        .RegisterNavigationInterceptor<AuthInterceptor>();

```

</details>

#### Config
```csharp

  var services = new ServiceCollection();
  services.AddNavigationSupport()
          .RegisterView<AView, AViewModel>("AView")
          .RegisterView<BView, BViewModel>("BView");
          .RegisterNavigation<CView, NavigationAware>("CNavigation");
          .RegisterDialog<CView, DialogAware>("CDialog");
          .RegisterDialogWindow<AWindow, AViewModel>("AWindow");

```
#### Execute

<details>
<summary>Code Examples</summary>

```csharp

  private readonly IRegionManager _regionManager;
  private readonly IDialogService _dialogService;

  public MainWindowViewModel(IRegionManager regionManager, IDialogService dialogService)
  {
      _regionManager = regionManager;
      _dialogService = dialogService;
  }

  [ReactiveCommand]
  private async Task AsyncNavigate(string param)
  {
      var result = await _regionManager.RequestNavigateAsync("MainRegion", "AView");
  }

  [ReactiveCommand]
  private void Show(string param)
  {
      _dialogService.ShowView("AView", callback: result => 
      {
          Debug.WriteLine(result.Result);
      });
  }

  [ReactiveCommand]
  private void ShowWindow(string param)
  {
      var result = _dialogService.ShowWindowDialog(param);
  }

  [ReactiveCommand]
  private async Task AsyncShowDialog(string param)
  {
      var result = await _dialogService.ShowViewDialogAsync("AView");
  }

  [ReactiveCommand]
  private async Task GoForward()
  {
      await _regionManager.GoForwardAsync("MainRegion");
  }

  [ReactiveCommand]
  private async Task GoBack()
  {
      await _regionManager.GoBackAsync("MainRegion");
  }

```
</details>

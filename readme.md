# 🚀 AsyncNavigation

[中文文档](readme_zh-cn.md)

> A lightweight asynchronous navigation framework based on **Microsoft.Extensions.DependencyInjection**

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

public class SampleViewModel : INavigationAware
{
    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;

    public virtual Task InitializeAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task<bool> IsNavigationTargetAsync(NavigationContext context)
    {
        return Task.FromResult(true);
    }

    public virtual async Task OnNavigatedFromAsync(NavigationContext context)
    {
        await Task.Delay(100, context.CancellationToken);
    }

    public virtual async Task OnNavigatedToAsync(NavigationContext context)
    {
        await Task.Delay(100, context.CancellationToken);
    }

    public virtual Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected Task RequestUnloadAsync()
    {
        if (AsyncRequestUnloadEvent == null)
        {
            return Task.CompletedTask;
        }
        return AsyncRequestUnloadEvent!.Invoke(this, AsyncEventArgs.Empty);
    }
}

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
      await _regionManager.GoForward("MainRegion");
  }

  [ReactiveCommand]
  private async Task GoBack()
  {
      await _regionManager.GoBack("MainRegion");
  }

```
</details>

# 🚀 AsyncNavigation


> 基于 **Microsoft.Extensions.DependencyInjection** 的轻量级异步导航框架

---

## ✨ 功能特性

-  **完全异步导航支持**  
  原生支持 `async/await`，让页面导航与异步任务协同更加自然与简洁。

-  **内置 DialogService**  
  通过异步方法轻松实现对话框导航。

-  **支持取消操作**  
  内置 `CancellationToken` 支持，可在任意阶段安全地中断导航。

-  **可自定义导航指示器**  
  允许开发者自定义导航过程的视觉反馈，用于指示加载、异常或完成等状态。

-  **多种内置 Region 类型**  
  除常见的基于 `ContentControl` 的单页面导航外，还原生支持 `ItemsControl` 与 `TabControl` 导航。

-  **RegionAdapter 扩展机制**  
  通过自定义 `RegionAdapter`，可灵活扩展并实现个性化导航逻辑。

-  **精细化控制选项**  
  提供丰富的导航配置选项，让导航行为更贴合业务需求。

-  **生命周期自动管理**  
  自动处理视图的创建、缓存与释放，有效避免内存泄漏。

-  **抽象层高度聚合**  
  核心逻辑高度抽象化，减少平台相关代码，便于扩展与单元测试。

-  **依赖极少**  
  仅依赖 `Microsoft.Extensions.DependencyInjection.Abstractions (>= 8.0)`。

-  **框架无关**  
  不依赖任何特定 MVVM 框架，可自由集成至任意架构中。

-  **支持 Native Aot**  
  支持avalonia的aot编译和裁剪，无需任何额外配置.

---

## 📦 安装

### WPF
```bash
dotnet add package AsyncNavigation.Wpf
```

### Avaloniaui
```bash
dotnet add package AsyncNavigation.Avaloniaui
```

## ⚡ 快速开始

### 准备Region

##### 设置 Namespace
```
 xmlns:an="https://github.com/NeverMorewd/AsyncNavigation"
```
##### 设置 RegionName
```xml
 <ContentControl an:RegionManager.RegionName="MainRegion" />
```

### 准备ViewModel
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

### 配置
```csharp

  var services = new ServiceCollection();
  services.AddNavigationSupport()
          .RegisterView<AView, AViewModel>("AView")
          .RegisterView<BView, BViewModel>("BView");
          .RegisterNavigation<CView, NavigationAware>("CNavigation");
          .RegisterDialog<CView, DialogAware>("CDialog");

```
### 执行
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
      _dialogService.Show("AView", callback: result => 
      {
          Debug.WriteLine(result.Result);
      });
  }
  [ReactiveCommand]
  private async Task AsyncShowDialog(string param)
  {
      var result = await _dialogService.ShowDialogAsync("AView");
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

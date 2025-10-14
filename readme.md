# 🚀 AsyncNavigation

[English](readme.md) | [中文文档](readme_zh-cn.md)

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

-  **RegionAdapter Extension Mechanism**  
  Extend and customize navigation behaviors by implementing your own `RegionAdapter`.

-  **Fine-grained Control Options**  
  Offers rich configuration options to make navigation behavior align precisely with application needs.

-  **Lifecycle Management**  
  Automatically handles view creation, caching, and release — effectively preventing memory leaks.

-  **Highly Abstract Core Layer**  
  Core logic is encapsulated within abstractions, minimizing platform-specific code and improving testability.

-  **Minimal Dependencies**  
  Depends only on `Microsoft.Extensions.DependencyInjection.Abstractions (>= 8.0)`.

- 🕊 **Framework-Agnostic**  
  Works with any MVVM framework, giving developers complete freedom of choice.

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


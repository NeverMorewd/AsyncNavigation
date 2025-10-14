# 🚀 AsyncNavigation

[English](./README.md) | [中文文档](./README_zh-cn.md)

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

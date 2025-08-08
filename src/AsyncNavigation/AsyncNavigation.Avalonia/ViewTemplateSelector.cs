using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Avalonia;

//public class ViewTemplateSelector : IDataTemplate
//{
//    private readonly IServiceProvider _serviceProvider;
//    //private readonly IRegionNavigationService<Control> _regionNavigationService;

//    public ViewTemplateSelector(IServiceProvider serviceProvider)
//    {
//        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
//        _regionNavigationService = _serviceProvider.GetRequiredService<IRegionNavigationService<Control>>();
//    }

//    public Control Build(object? param)
//    {
//        if (param is not NavigationContext navigationContext)
//        {
//            throw new ArgumentException($"Parameter must be of type {nameof(NavigationContext)}", nameof(param));
//        }

//        if (string.IsNullOrEmpty(navigationContext.ViewName))
//        {
//            throw new ArgumentException("ViewName cannot be null or empty", nameof(param));
//        }
//        var view = _regionNavigationService.RequestNavigateAsync(navigationContext).WaitOnDispatcherFrame();
//        return view;
//    }

//    public bool Match(object? data)
//    {
//        return data is NavigationContext;
//    }

//    public async Task WaitAsync(NavigationContext navigationContext)
//    {
//        await _regionNavigationService.WaitNavigationAsync(navigationContext);
//    }
//}
using AsyncNavigation.Avalonia;
using Avalonia.Controls;
using System;

namespace Sample.Avalonia.Regions
{
    public class ListBoxRegion : ItemsRegion
    {
        public ListBoxRegion(ItemsControl itemsControl, 
            IServiceProvider serviceProvider, 
            bool? useCache) : base(itemsControl, serviceProvider, useCache)
        {

        }
    }
}

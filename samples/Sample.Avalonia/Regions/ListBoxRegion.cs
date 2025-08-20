using AsyncNavigation.Abstractions;
using AsyncNavigation.Avalonia;
using AsyncNavigation.Core;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Avalonia.Regions
{
    public class ListBoxRegion : ItemsRegion
    {
        public ListBoxRegion(string name, 
            ItemsControl itemsControl, 
            IServiceProvider serviceProvider, 
            bool? useCache) : base(name, itemsControl, serviceProvider, useCache)
        {
        }
    }
}

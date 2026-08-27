using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace AsyncNavigation.WinUI;

internal static class XamlTemplateHelper
{
    public static DataTemplate CreateIndicatorHostTemplate()
    {
        const string xaml = """
    <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
        <ContentPresenter
            HorizontalAlignment="Stretch"
            VerticalAlignment="Stretch"
            HorizontalContentAlignment="Stretch"
            VerticalContentAlignment="Stretch"
            Content="{Binding IndicatorHost.Value.Host}" />
    </DataTemplate>
    """;

        return (DataTemplate)XamlReader.Load(xaml);
    }

    public static DataTemplate CreateTabItemTemplate()
    {
        const string xaml = """
    <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
        <TabViewItem
            HorizontalContentAlignment="Stretch"
            VerticalContentAlignment="Stretch"
            Header="{Binding ViewName}"
            Content="{Binding IndicatorHost.Value.Host}" />
    </DataTemplate>
    """;
        return (DataTemplate)XamlReader.Load(xaml);
    }
}

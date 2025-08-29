using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sample.Wpf.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            bool invert = Invert;
            if (parameter != null && bool.TryParse(parameter.ToString(), out bool paramInvert))
            {
                invert = paramInvert;
            }

            if (invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool invert = Invert;

            if (parameter != null && bool.TryParse(parameter.ToString(), out bool paramInvert))
            {
                invert = paramInvert;
            }

            bool result = visibility == Visibility.Visible;
            return invert ? !result : result;
        }
        return false;
    }
}

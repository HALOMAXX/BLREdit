using System;
using System.Globalization;
using System.Windows;

namespace BLREdit.API.Utils;

[System.Windows.Localizability(System.Windows.LocalizationCategory.NeverLocalize)]
public sealed class BooleanToNotVisibilityConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean && boolean)
        {
            return Visibility.Collapsed;
        }
        else
        {
            return Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visi && visi == Visibility.Visible)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

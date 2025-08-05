using System;
using System.Globalization;
using System.Windows;

namespace BLREdit.API.Utils;

[System.Windows.Localizability(System.Windows.LocalizationCategory.NeverLocalize)]
public sealed class BooleanToToggleButtonContentConverter : System.Windows.Data.IValueConverter
{
    //TODO: Add Translation Functionality
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean && boolean)
        {
            return "Enabled";
        }
        else
        {
            return "Enable";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string content && content == "Enabled")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

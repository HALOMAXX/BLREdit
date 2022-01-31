using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace BLREdit
{
    public class ComboBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ComboBox combo && parameter is TextBlock text)
            {
                return text.DataContext.GetType().GetProperty(combo.DisplayMemberPath).GetValue(combo, null);
            }
            else
            { return "Not Valid"; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}

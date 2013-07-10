using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Breadcrumbs
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool valueBool = (value as bool?) ?? false;

            if (string.Equals("invert", parameter))
            {
                valueBool = !valueBool;
            }

            return (valueBool ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

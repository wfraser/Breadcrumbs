﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DashMap
{
    public class DebugValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

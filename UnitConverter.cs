using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Breadcrumbs
{
    public class UnitConverter : IValueConverter
    {
        public ViewModels.MainViewModel MainVM { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? meters = value as double?;

            if (!meters.HasValue)
                return "unknown";

            double output;
            string units;

            switch (MainVM.UnitMode)
            {
                case UnitMode.Metric:
                    output = Math.Floor(meters.Value);
                    units = "m";

                    if (output >= 10000)
                    {
                        output /= 1000;
                        units = "km";
                    }
                    break;

                case UnitMode.Imperial:
                    output = Math.Floor(meters.Value * 3.28084);
                    units = "ft";

                    if (output >= 5280)
                    {
                        output /= 5280;
                        units = "mi";
                    }
                    break;

                default:
                    throw new InvalidOperationException("invalid unit mode");
            }

            return output.ToString() + " " + units;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

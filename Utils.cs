using System;
using System.Device.Location;
using System.Diagnostics;
using System.Text;
using System.Windows;
using Microsoft.Phone.Maps.Controls;
using Windows.Devices.Geolocation;

namespace Breadcrumbs
{
    public static class Utils
    {
        public static readonly double EarthRadiusMeters = 6371008.7714; // WGS84 mean radius

        public static string ToDMS(double degrees)
        {
            double minutes = (degrees - Math.Floor(degrees)) * 60.0;
            double seconds = (minutes - Math.Floor(minutes)) * 60.0;
            double tenths = (seconds - Math.Floor(seconds)) * 10.0;
            return string.Format(
                "{0}° {1}' {2}.{3}\"",
                Math.Floor(degrees),
                Math.Floor(minutes),
                Math.Floor(seconds),
                Math.Floor(tenths));
        }

        public static GeoCoordinate ConvertGeocoordinate(Geocoordinate coord)
        {
            if (coord == null)
                return null;

            return new GeoCoordinate(
                coord.Latitude,
                coord.Longitude,
                coord.Altitude ?? double.NaN,
                coord.Accuracy,
                coord.AltitudeAccuracy ?? double.NaN,
                coord.Speed ?? double.NaN,
                coord.Heading ?? double.NaN);
        }

        public static GeoCoordinate ConvertGeocoordinate(GeocoordinateEx coord)
        {
            if (coord == null)
                return null;

            return new GeoCoordinate(
                coord.Latitude,
                coord.Longitude,
                coord.Altitude ?? double.NaN,
                coord.Accuracy,
                coord.AltitudeAccuracy ?? double.NaN,
                coord.Speed ?? double.NaN,
                coord.Heading ?? double.NaN);
        }

        public static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        public static double ToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        public static GeoCoordinateCollection MakeCircle(GeoCoordinate center, double radiusMeters)
        {
            if (center == null)
            {
                Utils.ShowError("Center can't be null");
                return new GeoCoordinateCollection();
            }

            double lat = ToRadians(center.Latitude);
            double lng = ToRadians(center.Longitude);
            double angularRadius = radiusMeters / EarthRadiusMeters;
            var collection = new GeoCoordinateCollection();

            for (int x = 0; x < 360; x++) // 360 points might be overkill...
            {
                double circleRadians = ToRadians(x);
                double latRadians = Math.Asin(
                                        Math.Sin(lat) * Math.Cos(angularRadius)
                                        + Math.Cos(lat) * Math.Sin(angularRadius) * Math.Cos(circleRadians)
                                        );
                double lngRadians = lng + Math.Atan2(
                                        Math.Sin(circleRadians) * Math.Sin(angularRadius) * Math.Cos(lat),
                                        Math.Cos(angularRadius) - Math.Sin(lat) * Math.Sin(latRadians)
                                        );
                collection.Add(new GeoCoordinate(ToDegrees(latRadians), ToDegrees(lngRadians)));
            }

            return collection;
        }

        public static double GreatCircleDistance(GeocoordinateEx p1, GeocoordinateEx p2)
        {
            if (p1 == null || p2 == null)
            {
                Utils.ShowError("Both coordinates must be non-null");
                return 0.0;
            }

            double lat1 = ToRadians(p1.Latitude);
            double lng1 = ToRadians(p1.Longitude);
            double lat2 = ToRadians(p2.Latitude);
            double lng2 = ToRadians(p2.Latitude);
            double deltaLng = Math.Abs(lng1 - lng2);

            double centralAngle = Math.Atan2(
                Math.Sqrt(
                    Math.Pow(
                        Math.Cos(lat2) * Math.Sin(deltaLng), 2)
                    + Math.Pow(
                        Math.Cos(lat1) * Math.Sin(lat2)
                        - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLng), 2)),
                Math.Sin(lat1) * Math.Sin(lat2)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(deltaLng)
            );

            return centralAngle * EarthRadiusMeters;
        }

        public static void ShowError(string message, string caption = "Breadcumbs")
        {
            var stack = new StackTrace(skipFrames: 1);
            message = "Error: " + message + "\n\n" + stack.ToString();
            ShowErrorMessage(message, caption);
        }

        public static void ShowError(Exception ex, string caption = "Breadcrumbs")
        {
            ShowError(new AggregateException(ex));
        }

        public static void ShowError(AggregateException ex, string caption = "Breadcrumbs")
        {
            string message = string.Empty;
            if (ex == null)
            {
                message = "Empty exception!";
            }
            else
            {
                var sb = new StringBuilder();
                foreach (Exception inner in ex.InnerExceptions)
                {
                    sb.AppendLine(inner.Message);
                }
                message = sb.ToString();
            }

            ShowErrorMessage(message, caption);
        }

        private static void ShowErrorMessage(string message, string caption)
        {
            Debugger.Break();
            App.RootFrame.Dispatcher.BeginInvoke(() =>
                MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK));
        }
    }
}

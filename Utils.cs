using System;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using Windows.Devices.Geolocation;

namespace DashMap
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

        public static void ShowError(AggregateException ex, string caption = "DashMap")
        {
            string message = string.Empty;
            foreach (Exception inner in ex.InnerExceptions)
            {
                message += inner.Message + "\n";
            }

            App.RootFrame.Dispatcher.BeginInvoke(() =>
                System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK));
        }
    }
}

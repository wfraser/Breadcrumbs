using System;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using Windows.Devices.Geolocation;

namespace DashMap
{
    public static class Utils
    {
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
            int earthRadiusMeters = 6367000;
            double lat = ToRadians(center.Latitude);
            double lng = ToRadians(center.Longitude);
            double angularRadius = radiusMeters / earthRadiusMeters;
            var collection = new GeoCoordinateCollection();

            for (int x = 0; x <= 360; x++)
            {
                // This is some Ph.D. math right here. (Okay, not really, but don't fuck with it.)
                double circleRadians = ToRadians(x);
                double latRadians = Math.Asin(Math.Sin(lat) * Math.Cos(angularRadius) + Math.Cos(lat) * Math.Sin(angularRadius) * Math.Cos(circleRadians));
                double lngRadians = lng + Math.Atan2(Math.Sin(circleRadians) * Math.Sin(angularRadius) * Math.Cos(lat), Math.Cos(angularRadius) - Math.Sin(lat) * Math.Sin(latRadians));
                collection.Add(new GeoCoordinate(ToDegrees(latRadians), ToDegrees(lngRadians)));
            }

            return collection;
        }
    }
}

using System;
using System.Device.Location;
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
                "{0}°{1}'{2}.{3}\"",
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
    }
}

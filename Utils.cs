using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        public static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        public static double ToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        public static double MetersToFeet(double meters)
        {
            return meters * 3.28084;
        }

        public static double FeetToMiles(double feet)
        {
            return feet / 5280;
        }

        public static double MetersPerSecondToKMH(double metersPerSecond)
        {
            return metersPerSecond * 3.6;
        }

        public static double MetersPerSecondToMPH(double metersPerSecond)
        {
            return metersPerSecond * 2.237;
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
            double lng2 = ToRadians(p2.Longitude);
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

        public static async Task LockStepAsync<T>(
            IEnumerable<T> list1, IEnumerable<T> list2,
            Func<T, IComparable> orderBy,
            Func<T, T, double, Task> action,
            bool runInOrder = true)
        {
            var tasks = new List<Task>();

            List<T> lA = list1.OrderBy(orderBy).ToList();
            List<T> lB = list2.OrderBy(orderBy).ToList();

            int a = 0;
            int b = 0;
            while (a < lA.Count || b < lB.Count)
            {
                double progress = (double)(a + b) / (lA.Count + lB.Count);

                if (a == lA.Count)
                {
                    // Ran out of entries in lA; run action on the rest of lB
                    Task t = action(default(T), lB[b], progress);
                    if (runInOrder)
                        await t;
                    else
                        tasks.Add(t);
                    b++;
                }
                else if (b == lB.Count)
                {
                    // Ran out of entries in lB; run action on the rest of lA
                    Task t = action(lA[a], default(T), progress);
                    if (runInOrder)
                        await t;
                    else
                        tasks.Add(t);
                    a++;
                }
                else
                {
                    T entryA = lA[a];
                    T entryB = lB[b];

                    int compareResult = orderBy(entryA).CompareTo(orderBy(entryB));
                    if (compareResult == 0)
                    {
                        Task t = action(entryA, entryB, progress);
                        if (runInOrder)
                            await t;
                        else
                            tasks.Add(t);
                        a++;
                        b++;
                    }
                    else if (compareResult < 0)
                    {
                        // Entry A comes first.
                        Task t = action(entryA, default(T), progress);
                        if (runInOrder)
                            await t;
                        else
                            tasks.Add(t);
                        a++;
                    }
                    else if (compareResult > 0)
                    {
                        // Entry B comes first.
                        Task t = action(default(T), entryB, progress);
                        if (runInOrder)
                            await t;
                        else
                            tasks.Add(t);
                        b++;
                    }
                }
            }

            await Task.WhenAll(tasks);
        }

        public static async Task LockStepAsync<T>(
            IEnumerable<T> listA, IEnumerable<T> listB,
            Func<T, IComparable> orderBy,
            Func<T, T, Task> action,
            bool runInOrder = true)
        {
            await LockStepAsync(listA, listB, orderBy, (a, b, progress) => action(a, b), runInOrder);
        }

        public static void LockStep<T>(
            IEnumerable<T> listA, IEnumerable<T> listB,
            Func<T, IComparable> orderBy,
            Action<T, T, double> action)
        {
            Task.Run(
                async () => await LockStepAsync(listA, listB, orderBy,
                    (a, b, progress) => Task.Run(() => action(a, b, progress)),
                    runInOrder: true))
                .Wait();
        }

        public static void LockStep<T>(
            IEnumerable<T> listA, IEnumerable<T> listB,
            Func<T, IComparable> orderBy,
            Action<T, T> action)
        {
            LockStep(listA, listB, orderBy, (a, b, progress) => action(a, b));
        }

        /// <summary>
        /// Returns the local time (without DST correction) when the app was built.
        /// Relies on the Visual Studio automatic version number generation.
        /// </summary>
        private static DateTime GetBuildDateFromVersion(Version version)
        {
            return new DateTime(2000, 1, 1)
                + TimeSpan.FromDays(version.Build)
                + TimeSpan.FromSeconds(version.Revision * 2);
        }

        public static string GetFullVersion()
        {
            if (s_cachedFullVersion == null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string fileVersion = assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().First().InformationalVersion;
                Version assemblyVersion = assembly.GetName().Version;
                DateTime buildDate = GetBuildDateFromVersion(assemblyVersion);
                string suffix = 
#if DEBUG
                    "d";
#else
                    "R";
#endif

                s_cachedFullVersion = string.Format("{0}.{1:yyyyMMdd-HHmm}-{2}", fileVersion, buildDate, suffix);
            }
            return s_cachedFullVersion;
        }

        private static string s_cachedFullVersion = null;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;

namespace Breadcrumbs
{
    public static class ExtensionMethods
    {
        public static async Task<IStorageFile> CreatePathAsync(this IStorageFolder folder, string path, CreationCollisionOption collisionOption)
        {
            string[] parts = path.Split(new char[] { '\\', '/' });

            IStorageFolder currFolder = folder;
            foreach (string name in parts.Take(parts.Length - 1)) // Directory components, minus filename.
            {
                IStorageFolder subFolder = null;
                try
                {
                    subFolder = await currFolder.GetFolderAsync(name);
                }
                catch (FileNotFoundException)
                {
                    // Do nothing, handle below.
                }

                if (subFolder == null)
                {
                    subFolder = await currFolder.CreateFolderAsync(name, collisionOption);
                }
                currFolder = subFolder;
            }

            string filename = parts.Last();
            return await currFolder.CreateFileAsync(filename, collisionOption);
        }

        public static bool HasDescendant(this FrameworkElement parent, FrameworkElement child)
        {
            FrameworkElement current = child;
            do
            {
                if (current == parent)
                    return true;
                current = current.Parent as FrameworkElement;
            }
            while (current != null);
            return false;
        }

        private static double MinMaxOrDefault(IEnumerable<double?> values, double def, Func<double, double, double> selector, double startingValue)
        {
            bool haveValue = false;
            double selected = startingValue;
            foreach (double? dq in values)
            {
                if (dq.HasValue)
                {
                    haveValue = true;
                    selected = selector(selected, dq.Value);
                }
            }

            if (haveValue)
                return selected;
            else
                return def;
        }

        public static double MaxOrDefault(this IEnumerable<double?> values, double def)
        {
            return MinMaxOrDefault(values, def, Math.Max, double.MinValue);
        }

        public static double MinOrDefault(this IEnumerable<double?> values, double def)
        {
            return MinMaxOrDefault(values, def, Math.Min, double.MaxValue);
        }
    }
}

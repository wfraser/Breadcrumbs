using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Breadcrumbs
{
    public static class FileExtensions
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
    }
}

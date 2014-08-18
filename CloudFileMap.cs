using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;

namespace Breadcrumbs
{
    [XmlRoot("CloudFileMap", Namespace = CloudFileMap.Namespace)]
    public class CloudFileMap
    {
        public const string Namespace = "http://www.codewise.org/Breadcrumbs/CloudFileMap/1";

        [XmlElement("File")]
        public List<Mapping> Files
        {
            get { return m_files; }
        }
        private List<Mapping> m_files;

        private IStorageFile m_backingFile;

        // DO NOT USE THIS METHOD DIRECTLY.
        // It is for the deserializer's use only!
        public CloudFileMap()
        {
            m_files = new List<Mapping>();
        }

        public static async Task<CloudFileMap> CreateInstance(IStorageFile backingFile)
        {
            CloudFileMap instance = null;
            try
            {
                using (Stream s = await backingFile.OpenStreamForReadAsync())
                {
                    instance = Deserialize(s);
                }
                instance.m_backingFile = backingFile;
            }
            catch (Exception)
            {
                // If anything went wrong during deserialization, make a new mapping file below.
            }

            if (instance == null)
            {
                instance = new CloudFileMap();
                instance.m_backingFile = backingFile;
                await instance.Save();
            }

            return instance;
        }

        public Mapping GetMapping(string path)
        {
            return m_files.Where(mapping => mapping.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
        }

        public async Task AddMapping(string path, DateTime localModTime, string oneDriveId, DateTime oneDriveModTime)
        {
            m_files.Add(new Mapping()
            {
                Path = path,
                LocalModifiedTime = localModTime,
                OneDriveId = oneDriveId,
                OneDriveModifiedTime = oneDriveModTime,
            });
            await Save();
        }

        public async Task RemoveMappingsWhere(Predicate<Mapping> predicate)
        {
            int removed = m_files.RemoveAll(predicate);
            if (removed > 0)
            {
                await Save();
            }
        }

        public async Task RemoveMapping(string path)
        {
            int removed = m_files.RemoveAll(mapping => mapping.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                await Save();
            }
        }

        private async Task Save()
        {
            using (Stream s = await m_backingFile.OpenStreamForWriteAsync())
            {
                Serialize(s, this);
                s.SetLength(s.Position);
            }
        }

        private static void Serialize(Stream stream, CloudFileMap instance)
        {
            var settings = new XmlWriterSettings();
            settings.Indent = true;

            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(typeof(CloudFileMap), Namespace);
                serializer.Serialize(writer, instance);
            }
        }

        private static CloudFileMap Deserialize(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(CloudFileMap), Namespace);
            var result = (CloudFileMap)serializer.Deserialize(stream);
            return result;
        }

        #region Inner Classes

        public class Mapping
        {
            [XmlAttribute]
            public string Path
            {
                get;
                set;
            }

            [XmlElement("SkyDriveId")]
            public string OneDriveId
            {
                get;
                set;
            }

            [XmlElement("LocalModifiedTime")]
            public string LocalModifiedTimeString
            {
                get { return LocalModifiedTime.ToUniversalTime().ToString("o"); }
                set { LocalModifiedTime = DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind); }
            }

            [XmlIgnore]
            public DateTime LocalModifiedTime
            {
                get;
                set;
            }

            [XmlElement("SkyDriveModifiedTime")]
            public string OneDriveModifiedTimeString
            {
                get { return OneDriveModifiedTime.ToUniversalTime().ToString("o"); }
                set { OneDriveModifiedTime = DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind); }
            }

            [XmlIgnore]
            public DateTime OneDriveModifiedTime
            {
                get;
                set;
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Windows.Phone.Storage.SharedAccess;

namespace Breadcrumbs
{
    internal class UriMapper : UriMapperBase
    {
        public string IncomingFileName
        {
            get { return m_incomingFileName; }
        }
        private string m_incomingFileName;

        public string IncomingFileID
        {
            get { return m_incomingFileId; }
        }
        private string m_incomingFileId;

        private static readonly Uri DefaultPage = new Uri("/MainPage.xaml", UriKind.Relative);

        public override Uri MapUri(Uri uri)
        {
            string uriStr = uri.ToString();

            if (uriStr.StartsWith("/FileTypeAssociation"))
            {
                Match m = Regex.Match(uriStr, @"^/FileTypeAssociation\?fileToken=(?<id>.*)$");

                System.Diagnostics.Debug.Assert(m.Success, "FileTypeAssociation string match failed");
                if (!m.Success)
                {
                    return DefaultPage;
                }

                string fileId = m.Groups["id"].Value;

                string incomingFileName = SharedStorageAccessManager.GetSharedFileName(fileId);

                string extension = Path.GetExtension(incomingFileName).ToLower();
                System.Diagnostics.Debug.Assert(extension == ".gpx", "Incoming file name not .gpx");
                if (extension != ".gpx")
                {
                    return DefaultPage;
                }

                m_incomingFileId = fileId;
                m_incomingFileName = incomingFileName;

                // Let MainPage.xaml handle these.
                return new Uri("/MainPage.xaml", UriKind.Relative);
            }
            else
            {
                return uri;
            }
        }
    }
}

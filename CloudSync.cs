using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Live;
using Microsoft.Live.Controls;

namespace Breadcrumbs
{
    public class CloudSync
    {
        // Magic number from https://account.live.com/developers/applications/index
        private static readonly string ClientID = "000000004C0FB389";

        // What we name the GPX folder in the user's SkyDrive.
        private static readonly string GpxFolderName = "Breadcrumbs";

        private LiveConnectSession m_session;

        private async Task<LiveConnectSession> GetSession()
        {
            if (m_session != null && m_session.Expires > DateTime.Now)
            {
                return m_session;
            }

            try
            {
                // The browser window LoginAsync opens is forced into portrait, but the keyboard is
                // stuck in landscape mode, and it looks completely stupid.
                // So, we have to force portrait mode for the duration of the auth procedure.
                m_mainVM.ForcePortraitMode(true);

                var authClient = new LiveAuthClient(ClientID);
                LiveLoginResult result = await authClient.LoginAsync(
                    new string[] {
                        // Scopes and permsissions.
                        // See http://msdn.microsoft.com/en-us/library/live/hh243646.aspx
                        "wl.skydrive_update", // Read-write access to user's Skydrive
                        "wl.signin",          // Automatic signin, so they don't have to type a password.
                    });
                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    m_session = result.Session;
                    return result.Session;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
            finally
            {
                m_mainVM.ForcePortraitMode(false);
            }

            return null;
        }

        private async Task<string> GetGpxFolderId()
        {
            LiveConnectSession session = await GetSession();
            if (session == null)
            {
                return null;
            }

            var client = new LiveConnectClient(session);

            LiveOperationResult r;
            try
            {
                r = await client.GetAsync("me/skydrive/files");
            }
            catch (LiveConnectException ex)
            {
                Utils.ShowError(ex);
                return null;
            }

            var files = r.Result["data"] as IList<object>;
            if (files == null)
            {
                Utils.ShowError("Result from SkyDrive files query is an unknown type.");
                return null;
            }

            foreach (var folder in files.OfType<IDictionary<string, object>>())
            {
                if (!folder.Keys.Contains("name"))
                {
                    Utils.ShowError("A folder returned from SkyDrive has no Name field!");
                    return null;
                }
                else if (string.Equals(folder["name"], GpxFolderName))
                {
                    if (!folder.Keys.Contains("id"))
                    {
                        Utils.ShowError("GPX folder returned from SkyDrive has no ID field!");
                        return null;
                    }
                    else
                    {
                        return folder["id"] as string;
                    }
                }
            }

            // If we get to here, no GPX folder was found. Create one!

            var gpxFolder = new Dictionary<string, object>();
            gpxFolder.Add("name", GpxFolderName);

            try
            {
                r = await client.PostAsync("me/skydrive", gpxFolder);
            }
            catch (LiveConnectException ex)
            {
                Utils.ShowError(ex);
                return null;
            }

            var newFolder = r.Result as IDictionary<string, object>;
            if (newFolder == null)
            {
                Utils.ShowError("Newly created GPX folder is of unknown type.");
                return null;
            }
            else if (!newFolder.Keys.Contains("id"))
            {
                Utils.ShowError("Newly created GPX folder has no ID field!");
                return null;
            }
            else
            {
                return newFolder["id"] as string;
            }
        }

        public async Task<bool> Test()
        {
            string id = await GetGpxFolderId();
            return (id != null);
        }

        public CloudSync(ViewModels.MainViewModel mainVM)
        {
            m_mainVM = mainVM;
        }

        private ViewModels.MainViewModel m_mainVM;
    }
}

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

        public async Task<bool> Test()
        {
            var s = await GetSession();
            return (s != null);
        }

        public CloudSync(ViewModels.MainViewModel mainVM)
        {
            m_mainVM = mainVM;
        }

        private ViewModels.MainViewModel m_mainVM;
    }
}

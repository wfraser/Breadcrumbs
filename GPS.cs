using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;

namespace Breadcrumbs
{
    public class GPS
    {
        public GPS(ViewModels.MainViewModel mainVM)
        {
            m_mainVM = mainVM;
            m_haveFix = false;
        }

        public void Start()
        {
            m_geolocator = new Geolocator();
            m_geolocator.DesiredAccuracy = PositionAccuracy.High;
            m_geolocator.ReportInterval = 1000; // milliseconds
            m_geolocator.StatusChanged += Geolocator_StatusChanged;
            m_geolocator.PositionChanged += Geolocator_PositionChanged;

            // Disable the lock screen.
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            m_mainVM.IsGpsEnabled = true;
        }

        public void Stop()
        {
            m_geolocator.StatusChanged -= Geolocator_StatusChanged;
            m_geolocator.PositionChanged -= Geolocator_PositionChanged;
            m_geolocator = null;

            // Re-enable the lock screen.
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;

            m_mainVM.IsGpsEnabled = false;
            HaveFix = false;
        }

        void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            if (args.Position.Coordinate.PositionSource == PositionSource.Satellite)
            {
                m_mainVM.CurrentPosition = new GeocoordinateEx(args.Position.Coordinate);
                HaveFix = true;
            }
            else
            {
                // ignore Cellular and WiFi sources because they're not good enough for our purposes.
                HaveFix = false;
            }
        }

        void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            switch (args.Status)
            {
                case PositionStatus.NoData:
                case PositionStatus.Initializing:
                    HaveFix = false;
                    break;

                case PositionStatus.Disabled:
                    System.Windows.MessageBox.Show("The location service has been disabled. Please re-enable it in OS settings to use this app.");
                    break;

                case PositionStatus.NotAvailable:
                    System.Windows.MessageBox.Show("The location service is not available on this device.");
                    break;

                case PositionStatus.NotInitialized:
                case PositionStatus.Ready:
                    break;
            }
        }

        private bool HaveFix
        {
            set
            {
                if (m_haveFix != value)
                {
                    m_haveFix = value;
                    if (!value)
                    {
                        m_mainVM.GPX.NewTrackSegment();
                    }
                }
            }
        }

        private Geolocator m_geolocator;
        private ViewModels.MainViewModel m_mainVM;
        private bool m_haveFix;
    }
}

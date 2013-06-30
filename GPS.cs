using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;

namespace DashMap
{
    public class GPS
    {
        public GPS(ViewModels.MainViewModel mainVM)
        {
            m_mainVM = mainVM;
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
        }

        void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            m_mainVM.CurrentPosition = new GeocoordinateEx(args.Position.Coordinate);
        }

        void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            switch (args.Status)
            {
                //TODO: use this to create new track segments when GPS fix is lost.

                case PositionStatus.Disabled:
                case PositionStatus.Initializing:
                case PositionStatus.NoData:
                case PositionStatus.NotAvailable:
                case PositionStatus.NotInitialized:
                case PositionStatus.Ready:
                    break;
            }
        }

        private Geolocator m_geolocator;
        private ViewModels.MainViewModel m_mainVM;
    }
}

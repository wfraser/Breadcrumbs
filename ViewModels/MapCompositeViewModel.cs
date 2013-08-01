using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;

namespace Breadcrumbs.ViewModels
{
    public class MapCompositeViewModel : ViewModelBase
    {
        public MainViewModel MainVM
        {
            get { return m_mainVM; }
        }
        private MainViewModel m_mainVM;

        public string OverlayText
        {
            get { return m_overlayText; }
            set
            {
                m_overlayText = value;
                NotifyPropertyChanged("OverlayText");
            }
        }
        private string m_overlayText;

        public MapCartographicMode MapType
        {
            get { return m_mapType; }
            set
            {
                m_mapType = value;
                NotifyPropertyChanged("MapType");
            }
        }
        private MapCartographicMode m_mapType;

        public bool CenterOnCurrentPosition
        {
            get { return m_centerOnCurrentPosition; }
            set
            {
                m_centerOnCurrentPosition = value;
                NotifyPropertyChanged("CenterOnCurrentPosition");
            }
        }
        private bool m_centerOnCurrentPosition;

        public MapCompositeViewModel(MainViewModel mainVM)
        {
            m_mainVM = mainVM;
            m_overlayText = "Loading...";
            m_haveInitialPosition = false;
            m_centerOnCurrentPosition = true;

            m_mainVM.PropertyChanged += m_mainVM_PropertyChanged;
        }

        private bool m_haveInitialPosition;

        void m_mainVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPosition" && !m_haveInitialPosition)
            {
                m_haveInitialPosition = true;
                OverlayText = string.Empty;
            }
        }

        public async void GetInitialLocation()
        {
            if (true != (bool)System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings["LocationConsent"])
            {
                // User has opted out of location.
                return;
            }

            var geo = new Geolocator();
            geo.DesiredAccuracyInMeters = 50;

            OverlayText = "Finding your location...";

            try
            {
                Geoposition position = await geo.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10));

                m_mainVM.CurrentPosition = new GeocoordinateEx(position.Coordinate);
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // Location disabled
                }
                else
                {
                    // Something else went wrong!
                    System.Diagnostics.Debugger.Break();
                }
            }
        }
    }
}

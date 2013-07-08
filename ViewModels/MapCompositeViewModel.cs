using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;

namespace DashMap.ViewModels
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
            m_centerOnCurrentPosition = true;
        }

        public async Task<GeocoordinateEx> GetCurrentLocation()
        {
            if (true != (bool)System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings["LocationConsent"])
            {
                // User has opted out of location.
                return null;
            }

            var geo = new Geolocator();
            geo.DesiredAccuracyInMeters = 50;

            try
            {
                OverlayText = "Finding your location...";
                Geoposition position = await geo.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10));

                return new GeocoordinateEx(position.Coordinate);
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // Location disabled
                    return null;
                }
                else
                {
                    // Something else went wrong!
                    return null;
                }
            }
            finally
            {
                OverlayText = string.Empty;
            }
        }
    }
}

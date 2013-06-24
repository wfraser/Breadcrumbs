using System;
using System.Collections.ObjectModel;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using Windows.Devices.Geolocation;
using DashMap;
using DashMap.Resources;

namespace DashMap.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MapCompositeViewModel MapViewModel
        {
            get { return m_mapViewModel; }
        }

        public MapSidebarViewModel SidebarViewModel
        {
            get { return m_sidebarViewModel; }
        }

        public GPS GPS
        {
            get { return m_gps; }
        }

        public bool IsGpsEnabled
        {
            get { return m_isGpsEnabled; }
            set
            {
                m_isGpsEnabled = value;
                NotifyPropertyChanged("IsGpsEnabled");
            }
        }

        public bool IsTrackingEnabled
        {
            get { return m_isTrackingEnabled; }
            set
            {
                m_isTrackingEnabled = value;
                NotifyPropertyChanged("IsTrackingEnabled");
            }
        }

        public string Latitude
        {
            get
            {
                if (m_currentPosition == null)
                    return "-";

                double lat = m_currentPosition.Latitude;

                switch (m_coordMode)
                {
                    case CoordinateMode.Decimal:
                        return lat.ToString();
                    case CoordinateMode.DMS:
                        return Utils.ToDMS(lat);
                }
                return "error";
                //return "47.622348";
            }
        }

        public string Longitude
        {
            get
            {
                if (m_currentPosition == null)
                    return "-";

                double lng = m_currentPosition.Longitude;

                switch (m_coordMode)
                {
                    case CoordinateMode.Decimal:
                        return lng.ToString();
                    case CoordinateMode.DMS:
                        return Utils.ToDMS(lng);
                }
                return "error";
                //return "-122.325861";
            }
        }

        public string Altitude
        {
            get
            {
                if (m_currentPosition == null)
                    return "-";

                double? alt = m_currentPosition.Altitude;
                if (!alt.HasValue)
                    return "-";

                switch (m_units)
                {
                    case UnitMode.Metric:
                        return Math.Floor(alt.Value).ToString() + " m";
                    case UnitMode.Imperial:
                        return Math.Floor(alt.Value * 3.28084).ToString() + " ft";
                }
                return "error";
                //return "205 ft";
            }
        }

        public string Speed
        {
            get
            {
                if (m_currentPosition == null)
                    return "-";

                double? spd = m_currentPosition.Speed;
                if (!spd.HasValue)
                    return "-";

                if (double.IsNaN(spd.Value))
                    spd = 0.0;

                switch (m_units)
                {
                    case UnitMode.Metric:
                        return Math.Floor(spd.Value).ToString() + " km/h";
                    case UnitMode.Imperial:
                        return Math.Floor(spd.Value * 0.621371).ToString() + " MPH";
                }
                return "error";
                //return "88 MPH";
            }
        }

        public string Heading
        {
            get
            {
                if (m_currentPosition == null)
                    return "-";

                double? hdg = m_currentPosition.Heading;
                if (!hdg.HasValue || double.IsNaN(hdg.Value))
                    return "-";

                switch (m_coordMode)
                {
                    case CoordinateMode.Decimal:
                        return hdg.Value.ToString();
                    case CoordinateMode.DMS:
                        return Utils.ToDMS(hdg.Value);
                }
                return "error";
            }
        }

        public string Accuracy
        {
            get
            {
                if (m_currentPosition == null)
                    return "-";

                double accuracy = m_currentPosition.Accuracy;
                switch (m_units)
                {
                    case UnitMode.Metric:
                        return Math.Floor(accuracy).ToString() + " m";
                    case UnitMode.Imperial:
                        return Math.Floor(accuracy * 3.28084).ToString() + " ft";
                }
                return "error";
            }
        }

        public string Source
        {
            get
            {
                if (m_currentPosition == null)
                    return "No Data";

                switch (m_currentPosition.PositionSource)
                {
                    case PositionSource.Cellular:
                        return "Cellular";
                    case PositionSource.Satellite:
                        return "Satellite";
                    case PositionSource.WiFi:
                        return "WiFi";
                }
                return "error";
            }
        }

        public Geocoordinate CurrentPosition
        {
            get { return m_currentPosition; }
            set
            {
                m_currentPosition = value;
                NotifyPropertyChanged("CurrentPosition");
                NotifyPropertyChanged("CurrentGeoCoordinate");
                NotifyPropertyChanged("Latitude");
                NotifyPropertyChanged("Longitude");
                NotifyPropertyChanged("Altitude");
                NotifyPropertyChanged("Speed");
                NotifyPropertyChanged("Heading");
                NotifyPropertyChanged("Accuracy");
                NotifyPropertyChanged("Source");
            }
        }

        public GeoCoordinate CurrentGeoCoordinate
        {
            get
            {
                if (m_currentPosition == null)
                {
                    // This is the Map default
                    return new GeoCoordinate(
                        latitude: 0.0,          // Centered on equator
                        longitude: -88.86777,   // between North and South America
                        altitude: double.NaN,
                        horizontalAccuracy: double.NaN,
                        verticalAccuracy: double.NaN,
                        speed: double.NaN,
                        course: double.NaN
                        );
                }
                else
                {
                    return Utils.ConvertGeocoordinate(m_currentPosition);
                }
            }

            set
            {
                // ignored
                // We need this because the binding from CurrentGeoCoordinate to Map.Center
                // has to be TwoWay in order for it to work more than once. XAML WTF.
                // The canonical form of the current position is of type
                // Windows.Devices.Geolocation.Geoposition, which cannot be constructed or
                // modified.
            }
        }

        public MainViewModel()
        {
            m_mapViewModel = new MapCompositeViewModel(this);
            m_sidebarViewModel = new MapSidebarViewModel(this);
            m_isGpsEnabled = false;
            m_gps = new GPS(this);
            m_units = UnitMode.Imperial;
            m_coordMode = CoordinateMode.DMS;
        }

        public void ToggleGps()
        {
            if (m_isGpsEnabled)
            {
                m_gps.Stop();
            }
            else
            {
                m_gps.Start();
            }
        }

        public void ToggleTracking()
        {
            IsTrackingEnabled = !IsTrackingEnabled;
        }

        public event Action TrackCleared;
        public void ClearTrack()
        {
            var handler = TrackCleared;
            if (handler != null)
            {
                handler();
            }
        }

        public void CycleMapType()
        {
            MapCartographicMode mapType = m_mapViewModel.MapType;
            switch (mapType)
            {
                case MapCartographicMode.Aerial:
                    mapType = MapCartographicMode.Hybrid;
                    break;
                case MapCartographicMode.Hybrid:
                    mapType = MapCartographicMode.Road;
                    break;
                case MapCartographicMode.Road:
                    mapType = MapCartographicMode.Terrain;
                    break;
                case MapCartographicMode.Terrain:
                    mapType = MapCartographicMode.Aerial;
                    break;
            }
            m_mapViewModel.MapType = mapType;
        }

        private UnitMode m_units;
        private CoordinateMode m_coordMode;
        private bool m_isGpsEnabled;
        private bool m_isTrackingEnabled;
        private GPS m_gps;
        private MapCompositeViewModel m_mapViewModel;
        private MapSidebarViewModel m_sidebarViewModel;
        private Geocoordinate m_currentPosition;
    }
}
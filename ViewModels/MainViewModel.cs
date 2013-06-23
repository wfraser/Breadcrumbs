using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using Windows.Devices.Geolocation;
using DashMap;
using DashMap.Resources;

namespace DashMap.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
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

        public MapCompositeViewModel MapViewModel
        {
            get { return m_mapViewModel; }
        }

        public MapSidebarViewModel SidebarViewModel
        {
            get { return m_sidebarViewModel; }
        }

        public string Latitude
        {
            get
            {
                if (m_currentPosition == null)
                    return "-";

                double lat = m_currentPosition.Coordinate.Latitude;

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

                double lng = m_currentPosition.Coordinate.Longitude;

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

                double? alt = m_currentPosition.Coordinate.Altitude;
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

                double? spd = m_currentPosition.Coordinate.Speed;
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

                double? hdg = m_currentPosition.Coordinate.Heading;
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

                double accuracy = m_currentPosition.Coordinate.Accuracy;
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

                switch (m_currentPosition.Coordinate.PositionSource)
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

        public Geoposition CurrentPosition
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
                if (m_currentPosition == null || m_currentPosition.Coordinate == null)
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
                    return Utils.ConvertGeocoordinate(m_currentPosition.Coordinate);
                }
            }
        }

        public MainViewModel()
        {
            m_mapViewModel = new MapCompositeViewModel(this);
            m_sidebarViewModel = new MapSidebarViewModel(this);
            m_isGpsEnabled = false;
            m_gps = new GPS(this);
            m_units = UnitMode.Imperial;
            m_coordMode = CoordinateMode.Decimal;
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
        }

        public void ClearTrack()
        {
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                App.RootFrame.Dispatcher.BeginInvoke(() =>
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }

        private UnitMode m_units;
        private CoordinateMode m_coordMode;
        private bool m_isGpsEnabled;
        private GPS m_gps;
        private MapCompositeViewModel m_mapViewModel;
        private MapSidebarViewModel m_sidebarViewModel;
        private Geoposition m_currentPosition;
        private GeoCoordinate m_currentGeoCoordinate;
    }
}
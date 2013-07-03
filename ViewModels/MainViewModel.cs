using System;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Phone.Maps.Controls;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
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

        public FileBrowserViewModel FileBrowserViewModel
        {
            get { return m_fileBrowserViewModel; }
        }

        public GPS GPS
        {
            get { return m_gps; }
        }

        public GPX GPX
        {
            get { return m_gpx; }
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
                if (value != m_isTrackingEnabled)
                {
                    m_isTrackingEnabled = value;

                    if (m_isTrackingEnabled)
                    {
                        // Tracking turned on after being off.
                        // Make a new track segment.

                        m_gpx.NewTrackSegment();
                        // TODO: make new visual track segment here too
                    }

                    NotifyPropertyChanged("IsTrackingEnabled");
                }
            }
        }

        public bool IsFileBrowserVisible
        {
            get { return m_isFileBrowserVisible; }
            set
            {
                if (value != m_isFileBrowserVisible)
                {
                    m_isFileBrowserVisible = value;
                    NotifyPropertyChanged("IsFileBrowserVisible");
                }
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

                // Speed comes in as meters per second
                switch (m_units)
                {
                    case UnitMode.Metric:
                        return Math.Floor(spd.Value * 3.6).ToString() + " km/h";
                    case UnitMode.Imperial:
                        return Math.Floor(spd.Value * 2.237).ToString() + " MPH";
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

        public DateTimeOffset? Timestamp
        {
            get
            {
                if (m_currentPosition == null)
                    return null;

                return m_currentPosition.Timestamp;
            }
        }

        public GeocoordinateEx CurrentPosition
        {
            get
            {
                m_locationRead.Set();
                return m_currentPosition;
            }

            set
            {
                m_currentPosition = value;

                if (m_isTrackingEnabled)
                {
                    m_gpx.AddTrackPoint(m_currentPosition);
                }

                NotifyPropertyChanged("CurrentPosition");
                NotifyPropertyChanged("CurrentGeoCoordinate");
                NotifyPropertyChanged("Latitude");
                NotifyPropertyChanged("Longitude");
                NotifyPropertyChanged("Altitude");
                NotifyPropertyChanged("Speed");
                NotifyPropertyChanged("Heading");
                NotifyPropertyChanged("Accuracy");
                NotifyPropertyChanged("Source");
                NotifyPropertyChanged("Timestamp");
            }
        }

        public CoordinateMode CoordinateMode
        {
            get { return m_coordMode; }
            set
            {
                if (m_coordMode != value)
                {
                    m_coordMode = value;
                    NotifyPropertyChanged("Latitude");
                    NotifyPropertyChanged("Longitude");
                    NotifyPropertyChanged("Heading");
                }
            }
        }

        public UnitMode UnitMode
        {
            get { return m_units; }
            set
            {
                if (m_units != value)
                {
                    m_units = value;
                    NotifyPropertyChanged("Altitude");
                    NotifyPropertyChanged("Speed");
                    NotifyPropertyChanged("Accuracy");
                }
            }
        }

        internal ManualResetEvent LocationReadEvent
        {
            get { return m_locationRead; }
        }

        public MainViewModel()
        {
            m_mapViewModel = new MapCompositeViewModel(this);
            m_sidebarViewModel = new MapSidebarViewModel(this);
            m_isGpsEnabled = false;
            m_isFileBrowserVisible = false;
            m_gps = new GPS(this);
            m_gpx = new GPX();
            m_units = UnitMode.Imperial;
            m_coordMode = CoordinateMode.DMS;

            m_locationRead = new ManualResetEvent(false);
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
            m_gpx.ClearTracks();
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

        public void CycleUnits()
        {
            UnitMode mode = m_units;
            switch (mode)
            {
                case UnitMode.Imperial:
                    mode = UnitMode.Metric;
                    break;
                case UnitMode.Metric:
                    mode = UnitMode.Imperial;
                    break;
            }
            m_units = mode;
            NotifyPropertyChanged("Altitude");
            NotifyPropertyChanged("Accuracy");
            NotifyPropertyChanged("Speed");
        }

        public void CycleCoordinateMode()
        {
            CoordinateMode mode = m_coordMode;
            switch (mode)
            {
                case CoordinateMode.Decimal:
                    mode = CoordinateMode.DMS;
                    break;
                case CoordinateMode.DMS:
                    mode = CoordinateMode.Decimal;
                    break;
            }
            m_coordMode = mode;
            NotifyPropertyChanged("Latitude");
            NotifyPropertyChanged("Longitude");
            NotifyPropertyChanged("Heading");
        }

        public async void SaveTrack()
        {
            StorageFolder local = ApplicationData.Current.LocalFolder;

            StorageFolder gpxFolder = null;
            try
            {
                gpxFolder = await local.GetFolderAsync("GPX");
            }
            catch (FileNotFoundException)
            {
                // do nothing
            }

            if (gpxFolder == null)
            {
                gpxFolder = await local.CreateFolderAsync("GPX");
            }

            m_fileBrowserViewModel = new FileBrowserViewModel(
                this,
                gpxFolder,
                ViewModels.FileBrowserMode.Save,
                new Action<IStorageFile>(result =>
                {
                    if (result != null)
                    {
                        result.OpenStreamForWriteAsync()
                            .ContinueWith(prevTask =>
                            {
                                try
                                {
                                    using (Stream stream = prevTask.Result)
                                    {
                                        m_gpx.Serialize(stream);
                                    }
                                }
                                catch (AggregateException ex)
                                {
                                    Utils.ShowError(ex, "Error saving GPX");
                                }
                            });
                    }
                    // else: save was cancelled or an error occured upstream
                }));
            NotifyPropertyChanged("FileBrowserViewModel");
            IsFileBrowserVisible = true;
        }

        public string GetTrackGPX()
        {
            var gpxData = new MemoryStream();
            m_gpx.Serialize(gpxData);
            byte[] bytes = gpxData.ToArray();
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public void LoadTrack(string gpxData)
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(gpxData));
            m_gpx = GPX.Deserialize(stream);
        }

        private UnitMode m_units;
        private CoordinateMode m_coordMode;
        private bool m_isGpsEnabled;
        private bool m_isTrackingEnabled;
        private bool m_isFileBrowserVisible;
        private GPS m_gps;
        private GPX m_gpx;
        private MapCompositeViewModel m_mapViewModel;
        private MapSidebarViewModel m_sidebarViewModel;
        private FileBrowserViewModel m_fileBrowserViewModel;
        private GeocoordinateEx m_currentPosition;

        // For testing
        private ManualResetEvent m_locationRead;

    }
}
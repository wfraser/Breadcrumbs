using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Breadcrumbs
{
    //
    // Replacements for Windows.Devices.Geolocation.*
    // because they are annoyingly not activatable.
    //

    public class GeocoordinateEx
    {
        public Geocoordinate Geocoordinate
        {
            get { return m_real; }
        }

        public double Accuracy
        {
            get { return m_accuracy; }
            set { m_accuracy = value; }
        }

        public double? Altitude
        {
            get { return m_altitude; }
            set { m_altitude = value; }
        }

        public double? AltitudeAccuracy
        {
            get { return m_altitudeAccuracy; }
            set { m_altitudeAccuracy = value; }
        }

        public double? Heading
        {
            get { return m_heading; }
            set { m_heading = value; }
        }

        public double Latitude
        {
            get { return m_latitude; }
            set { m_latitude = value; }
        }

        public double Longitude
        {
            get { return m_longitude; }
            set { m_longitude = value; }
        }

        public PositionSource PositionSource
        {
            get { return m_positionSource; }
            set { m_positionSource = value; }
        }

        public GeocoordinateSatelliteDataEx SatelliteData
        {
            get { return m_satelliteData; }
            set { m_satelliteData = value; }
        }

        public double? Speed
        {
            get { return m_speed; }
            set { m_speed = value; }
        }

        public DateTimeOffset Timestamp
        {
            get { return m_timestamp; }
            set { m_timestamp = value; }
        }

        public System.Device.Location.GeoCoordinate GeoCoordinate
        {
            get
            {
                return new System.Device.Location.GeoCoordinate(
                    Latitude,
                    Longitude,
                    Altitude ?? double.NaN,
                    Accuracy,
                    AltitudeAccuracy ?? double.NaN,
                    Speed ?? double.NaN,
                    Heading ?? double.NaN);
            }
        }

        public GeocoordinateEx(Geocoordinate real)
        {
            if (real == null)
            {
                Utils.ShowError("Coordinate can't be null");
                return;
            }

            m_real = real;
            m_accuracy = real.Accuracy;
            m_altitude = real.Altitude;
            m_altitudeAccuracy = real.AltitudeAccuracy;
            m_heading = real.Heading;
            m_latitude = real.Latitude;
            m_longitude = real.Longitude;
            m_positionSource = real.PositionSource;
            m_satelliteData = new GeocoordinateSatelliteDataEx(real.SatelliteData);
            m_speed = real.Speed;
            m_timestamp = real.Timestamp;
        }

        public GeocoordinateEx(double latitude, double longitude, double? altitude = null)
        {
            m_real = null;
            m_latitude = latitude;
            m_longitude = longitude;
            m_altitude = altitude;

            m_altitudeAccuracy = ((altitude == null) ? null : (double?)1.0);
            m_accuracy = 1.0;
            m_heading = null;
            m_positionSource = Windows.Devices.Geolocation.PositionSource.Satellite;
            m_satelliteData = new GeocoordinateSatelliteDataEx(null, null, null);
            m_speed = null;
            m_timestamp = DateTimeOffset.Now;
        }

        private Geocoordinate m_real;
        private double m_accuracy;
        private double? m_altitude;
        private double? m_altitudeAccuracy;
        private double? m_heading;
        private double m_latitude;
        private double m_longitude;
        private PositionSource m_positionSource;
        private GeocoordinateSatelliteDataEx m_satelliteData;
        private double? m_speed;
        private DateTimeOffset m_timestamp;
    }

    public class GeocoordinateSatelliteDataEx
    {
        public GeocoordinateSatelliteData Real
        {
            get { return m_real; }
        }

        public double? HorizontalDilutionOfPrecision
        {
            get { return m_hdop; }
            set { m_hdop = value; }
        }

        public double? PositionDilutionOfPrecision
        {
            get { return m_pdop; }
            set { m_pdop = value; }
        }

        public double? VerticalDilutionOfPrecision
        {
            get { return m_vdop; }
            set { m_vdop = value; }
        }

        public GeocoordinateSatelliteDataEx(GeocoordinateSatelliteData real)
        {
            m_real = real;
            m_hdop = real.HorizontalDilutionOfPrecision;
            m_pdop = real.PositionDilutionOfPrecision;
            m_vdop = real.VerticalDilutionOfPrecision;
        }

        public GeocoordinateSatelliteDataEx(double? hdop, double? pdop, double? vdop)
        {
            m_real = null;
            m_hdop = hdop;
            m_pdop = pdop;
            m_vdop = vdop;
        }

        private GeocoordinateSatelliteData m_real;
        private double? m_hdop;
        private double? m_pdop;
        private double? m_vdop;
    }
}

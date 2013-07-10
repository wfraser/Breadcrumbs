using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Breadcrumbs
{
    //
    // This is a minimal implementation of GPX 1.1, just enough to save track data.
    //
    // Elements or attributes not represented here will be ignored on deserialization.
    //

    [XmlRoot("gpx", Namespace = "http://www.topografix.com/GPX/1/1")]
    public class GPX
    {
        public static readonly string Namespace = "http://www.topografix.com/GPX/1/1";

        [XmlAttribute("version")]
        public string Version
        {
            get;
            set;
        }

        [XmlAttribute("creator")]
        public string Creator
        {
            get;
            set;
        }

        [XmlElement("trk")]
        public List<Track> Tracks
        {
            get { return m_tracks; }
        }
        private List<Track> m_tracks;

        public GPX()
        {
            m_tracks = new List<Track>();
            Version = "1.1";
            Creator = "Breadcrumbs/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        public void Serialize(Stream stream)
        {
            // First, clean out any empty track segments.
            foreach (var track in m_tracks)
            {
                track.Segments.RemoveAll(seg => seg.Points.Count == 0);
            }

            var settings = new XmlWriterSettings();
            settings.Indent = true;

            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(typeof(GPX), Namespace);
                serializer.Serialize(writer, this);
            }
        }

        public static GPX Deserialize(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(GPX), Namespace);
            var gpx = (GPX)serializer.Deserialize(stream);
            return gpx;
        }

        public void NewTrackSegment()
        {
            if (m_tracks.Count == 0)
            {
                m_tracks.Add(new Track());
            }

            m_tracks[0].Segments.Add(new TrackSegment());
        }

        public void AddTrackPoint(GeocoordinateEx coordinate)
        {
            if (m_tracks.Count == 0)
            {
                NewTrackSegment();
            }

            var point = new TrackPoint()
            {
                Latitude = coordinate.Latitude,
                Longitude = coordinate.Longitude,
                Altitude = coordinate.Altitude,
                DateTime = coordinate.Timestamp.DateTime,
            };

            m_tracks[0].Segments[m_tracks[0].Segments.Count - 1].Points.Add(point);
        }

        public void ClearTracks()
        {
            m_tracks = new List<Track>();
        }

        #region Inner GPX Classes

        public class Track
        {
            [XmlElement("trkseg")]
            public List<TrackSegment> Segments
            {
                get { return m_segments; }
            }
            private List<TrackSegment> m_segments;

            public Track()
            {
                m_segments = new List<TrackSegment>();
            }
        }

        public class TrackSegment
        {
            [XmlElement("trkpt")]
            public List<TrackPoint> Points
            {
                get { return m_points; }
            }
            private List<TrackPoint> m_points;

            public TrackSegment()
            {
                m_points = new List<TrackPoint>();
            }
        }

        public class TrackPoint
        {
            [XmlAttribute("lat")]
            public double Latitude
            {
                get;
                set;
            }

            [XmlAttribute("lon")]
            public double Longitude
            {
                get;
                set;
            }

            [XmlElement("ele")]
            public double? Altitude
            {
                get;
                set;
            }

            [XmlIgnore]
            public bool AltitudeSpecified
            {
                // This tells the XML Serializer to not write Altitude if it's null.
                get { return Altitude.HasValue; }
            }

            [XmlElement("time")]
            public string DateTimeStr
            {
                // ISO 8601 date/time format: "2013-06-28T04:31:25.1234567Z"
                get { return DateTime.ToUniversalTime().ToString("o"); }
                set { DateTime = DateTime.Parse(value, null, DateTimeStyles.RoundtripKind); }
            }

            [XmlIgnore]
            public DateTime DateTime
            {
                get;
                set;
            }

            [XmlIgnore]
            public GeocoordinateEx GeocoordinateEx
            {
                get
                {
                    var coord = new GeocoordinateEx(Latitude, Longitude, Altitude);
                    coord.Timestamp = DateTime;
                    return coord;
                }
            }

            public TrackPoint()
            {
            }
        }

        #endregion Inner GPX Classes
    }
}
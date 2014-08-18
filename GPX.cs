using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Breadcrumbs
{
    //
    // This is a minimal implementation of GPX 1.1, just enough to save track data.
    //
    // Elements or attributes not represented here will be ignored on deserialization.
    //

    [XmlRoot("gpx", Namespace = GPX.Namespace)]
    public class GPX
    {
        public const string Namespace = "http://www.topografix.com/GPX/1/1";
        public const string OldNamespace = "http://www.topografix.com/GPX/1/0";

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

        [XmlIgnore]
        public bool Dirty
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
            Creator = "Breadcrumbs/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
                + " WindowsPhone/" + System.Environment.OSVersion.Version;
            Dirty = false;
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

            Dirty = false;
        }

        public static GPX Deserialize(Stream stream)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(GPX));
                var gpx = (GPX)serializer.Deserialize(stream);
                return gpx;
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException is InvalidOperationException && ex.InnerException.Message.Contains(GPX.OldNamespace))
                {
                    // This is a bit of a hack.
                    // If the deserialization fails due to the data being GPX 1.0,
                    // read in the file, change the namespace to GPX 1.1, and try again.
                    // I wish I could get the XML (de)serializer to accept either namespace...

                    string xml;
                    using (var memStream = new MemoryStream())
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(memStream);
                        xml = Encoding.UTF8.GetString(memStream.ToArray(), 0, (int)memStream.Length);
                    }
                    xml = xml.Replace(GPX.OldNamespace, GPX.Namespace);
                    using (var memStream = new MemoryStream())
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(xml);
                        memStream.Write(bytes, 0, bytes.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        return GPX.Deserialize(memStream);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public void NewTrackSegment()
        {
            if (m_tracks.Count == 0)
            {
                m_tracks.Add(new Track());
            }

            m_tracks[0].Segments.Add(new TrackSegment());
            Dirty = true;
        }

        public void AddTrackPoint(GeocoordinateEx coordinate)
        {
            if (coordinate == null)
            {
                Utils.ShowError("Coordinate can't be null");
                return;
            }

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
            Dirty = true;
        }

        public void ClearTracks()
        {
            m_tracks = new List<Track>();
            Dirty = false;
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
            public string DateTimeString
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
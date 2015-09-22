using System;
using System.Collections.Generic;
using System.Linq;
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

    [XmlRoot("gpx", Namespace = GPX.GPXNamespace)]
    public class GPX
    {
        public const string GPXNamespace = "http://www.topografix.com/GPX/1/1";
        public const string OldGPXNamespace = "http://www.topografix.com/GPX/1/0";
        public const string BreadcrumbsNamespace = "http://ns.codewise.org/Breadcrumbs/GPX/1/0";

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
            Creator = "Breadcrumbs/" + Utils.GetFullVersion()
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

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("b", BreadcrumbsNamespace);

            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(typeof(GPX), GPXNamespace);
                serializer.Serialize(writer, this, namespaces);
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
                if (ex.InnerException is InvalidOperationException && ex.InnerException.Message.Contains(GPX.OldGPXNamespace))
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
                    xml = xml.Replace(GPX.OldGPXNamespace, GPX.GPXNamespace);
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
            Track currentTrack;
            if (m_tracks.Count == 0)
            {
                currentTrack = new Track();
                m_tracks.Add(currentTrack);
            }
            else
            {
                currentTrack = m_tracks.Last();
            }

            if (currentTrack.Segments.Count == 0 || currentTrack.Segments.Last().Points.Count > 0)
            {
                currentTrack.Segments.Add(new TrackSegment());
                Dirty = true;
            }
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
                Accuracy = coordinate.Accuracy,
                AltitudeAccuracy = coordinate.AltitudeAccuracy
            };

            m_tracks.Last().AddPoint(point);
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

            [XmlIgnore]
            public double MinAltitude
            {
                get
                {
                    lock (m_minAltitudeMutex)
                    {
                        if (!m_minAltitude.HasValue)
                        {
                            double d = m_segments.SelectMany(seg => seg.Points)
                                         .Select(point => point.Altitude)
                                         .MinOrDefault(double.NaN);
                            if (d != double.NaN)
                                m_minAltitude = d;

                            return d;
                        }
                        else
                        {
                            return m_minAltitude.Value;
                        }
                    }
                }
            }
            private double? m_minAltitude;
            private object m_minAltitudeMutex = new object();

            [XmlIgnore]
            public double MaxAltitude
            {
                get
                {
                    lock (m_maxAltitudeMutex)
                    {
                        if (!m_maxAltitude.HasValue)
                        {
                            double d = m_segments.SelectMany(seg => seg.Points)
                                         .Select(point => point.Altitude)
                                         .MaxOrDefault(double.NaN);
                            if (d != double.NaN)
                                m_maxAltitude = d;

                            return d;
                        }
                        else
                        {
                            return m_maxAltitude.Value;
                        }
                    }
                }
            }
            private double? m_maxAltitude;
            private object m_maxAltitudeMutex = new object();

            [XmlIgnore]
            public double Distance
            {
                get
                {
                    lock (m_distanceMutex)
                    {
                        if (!m_distance.HasValue)
                        {
                            IEnumerable<GeocoordinateEx> coords = m_segments.SelectMany(seg => seg.Points)
                                    .Select(seg => seg.GeocoordinateEx);
                            double d = coords.Zip(coords.Skip(1), (a, b) => Utils.GreatCircleDistance(a, b)).Sum();
                            //TODO: what happens if there's only one point?
                            m_distance = d;
                            return d;
                        }
                        else
                        {
                            return m_distance.Value;
                        }
                    }
                }
            }
            private double? m_distance;
            private object m_distanceMutex = new object();

            public void AddPoint(TrackPoint point)
            {
                TrackSegment seg;

                if (m_segments.Count == 0)
                {
                    seg = new TrackSegment();
                    m_segments.Add(seg);
                }
                else
                {
                    seg = m_segments.Last();
                }

                // Update stats

                if (point.Altitude.HasValue)
                {
                    lock (m_maxAltitudeMutex)
                    {
                        if (m_maxAltitude.HasValue)
                            m_maxAltitude = Math.Max(m_maxAltitude.Value, point.Altitude.Value);
                    }
                    lock (m_minAltitudeMutex)
                    {
                        if (m_minAltitude.HasValue)
                            m_minAltitude = Math.Min(m_minAltitude.Value, point.Altitude.Value);
                    }
                }
                
                lock (m_distanceMutex)
                {
                    if (m_distance.HasValue && seg.Points.Count > 0)
                    {
                        m_distance += Utils.GreatCircleDistance(
                            seg.Points.Last().GeocoordinateEx,
                            point.GeocoordinateEx);
                    }
                }

                seg.Points.Add(point);
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

            [XmlElement("Accuracy", Namespace=BreadcrumbsNamespace)]
            public double Accuracy
            {
                get;
                set;
            }

            [XmlIgnore]
            public bool AccuracySpecified
            {
                get { return Accuracy > 0; }
            }

            [XmlElement("AltitudeAccuracy", Namespace=BreadcrumbsNamespace)]
            public double AltitudeAccuracyValue
            {
                get { return AltitudeAccuracy.HasValue ? AltitudeAccuracy.Value : double.NaN; }
                set { AltitudeAccuracy = value; }
            }

            [XmlIgnore]
            public double? AltitudeAccuracy
            {
                get;
                set;
            }

            [XmlIgnore]
            public bool AltitudeAccuracyValueSpecified
            {
                get { return AltitudeAccuracy.HasValue; }
            }

            [XmlIgnore]
            public GeocoordinateEx GeocoordinateEx
            {
                get
                {
                    var coord = new GeocoordinateEx(Latitude, Longitude, Altitude);
                    coord.Timestamp = DateTime;
                    coord.Accuracy = Accuracy;
                    coord.AltitudeAccuracy = AltitudeAccuracy;
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
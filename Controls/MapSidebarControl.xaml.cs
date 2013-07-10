using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DashMap
{
    public partial class MapSidebarControl : UserControl
    {
        static DependencyProperty IsGpsEnabledProperty = DependencyProperty.Register("IsGpsEnabled", typeof(bool), typeof(MapSidebarControl),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsGpsEnabledChanged)));
        static void OnIsGpsEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var sidebar = (MapSidebarControl)target;
            sidebar.GpsToggle.ImageSource = new Uri("/Assets/Icons/transport." + ((bool)e.NewValue ? "pause" : "play") + ".png", UriKind.Relative);
            sidebar.GpsToggle.IsEnabled = true;
        }
        public bool IsGpsEnabled
        {
            get { return (bool)GetValue(IsGpsEnabledProperty); }
            set { SetValue(IsGpsEnabledProperty, value); }
        }

        static DependencyProperty IsTrackingEnabledProperty = DependencyProperty.Register("IsTrackingEnabled", typeof(bool), typeof(MapSidebarControl),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsTrackingEnabledChanged)));
        static void OnIsTrackingEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var sidebar = (MapSidebarControl)target;
            sidebar.TrackToggle.ImageSource = new Uri("/Assets/Icons/transport." + ((bool)e.NewValue ? "pause" : "play") + ".png", UriKind.Relative);
            sidebar.TrackToggle.IsEnabled = true;
        }
        public bool IsTrackingEnabled
        {
            get { return (bool)GetValue(IsTrackingEnabledProperty); }
            set { SetValue(IsTrackingEnabledProperty, value); }
        }

        public MapSidebarControl()
        {
            InitializeComponent();
        }

        private void GpsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"])
            {
                if (!App.LocationConsentPrompt())
                {
                    // User doesn't want location.
                    return;
                }
            }

            GpsToggle.IsEnabled = false;
            m_viewModel.MainVM.ToggleGps();
        }

        private void TrackToggle_Click(object sender, RoutedEventArgs e)
        {
            TrackToggle.IsEnabled = false;
            m_viewModel.MainVM.ToggleTracking();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.MainVM.ClearTrack();
        }

        private void MapToggle_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.MainVM.CycleMapType();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel.IsExpanded)
            {
                MenuButton.ImageSource = new Uri("/Assets/Icons/next.png", UriKind.Relative);
                m_viewModel.IsExpanded = false;
            }
            else
            {
                MenuButton.ImageSource = new Uri("/Assets/Icons/back.png", UriKind.Relative);
                m_viewModel.IsExpanded = true;
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.MainVM.SaveTrack();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.MainVM.LoadTrack();
        }

        private void UnitToggleButton_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.MainVM.CycleUnits();
        }

        private void DMSToggleButton_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.MainVM.CycleCoordinateMode();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var thread = new System.Threading.Thread(
                new System.Threading.ThreadStart(
                    () =>
                    {
                        //
                        // Awesome demo code
                        //
                        var gpx = GPX.Deserialize(Application.GetResourceStream(new Uri("Assets/Test/rainier.gpx", UriKind.Relative)).Stream);
                        foreach (var seg in gpx.Tracks[0].Segments)
                        {
                            foreach (var point in seg.Points)
                            {
                                var coord = new GeocoordinateEx(
                                    point.Latitude, point.Longitude, point.Altitude);
                                coord.Timestamp = point.DateTime;
                                coord.PositionSource = Windows.Devices.Geolocation.PositionSource.Satellite;

                                m_viewModel.MainVM.LocationReadEvent.Reset();
                                m_viewModel.MainVM.CurrentPosition = coord;
                                m_viewModel.MainVM.LocationReadEvent.WaitOne();
                            }
                        }
                    }));
            thread.Start();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_viewModel = (ViewModels.MapSidebarViewModel)DataContext;
        }

        private ViewModels.MapSidebarViewModel m_viewModel;
    }
}

using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Breadcrumbs
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
                m_viewModel.IsExpanded = false;
            }
            else
            {
                m_viewModel.IsExpanded = true;
                ExpandedScrollViewer.Focus();
            }
        }

        private void SetMenuButtonState(bool menuIsExpanded)
        {
            if (menuIsExpanded)
            {
                MenuButton.ImageSource = new Uri("/Assets/Icons/back.png", UriKind.Relative);
            }
            else
            {
                MenuButton.ImageSource = new Uri("/Assets/Icons/next.png", UriKind.Relative);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.IsExpanded = false;
            m_viewModel.MainVM.SaveTrack();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.IsExpanded = false;
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

#if DEBUG
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
#endif

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version ver = assembly.GetName().Version;
            string copyright = assembly.GetCustomAttributes<AssemblyCopyrightAttribute>().First().Copyright;

            MessageBox.Show(
                string.Format("Breadcrumbs v{0}.{1} build {2} rev {3}\n", ver.Major, ver.Minor, ver.Build, ver.Revision)
                    + copyright
                    + "\nhttps://github.com/wfraser/Breadcrumbs"
                    + "\n\n:)",
                "About Breadcrumbs",
                MessageBoxButton.OK);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_viewModel = (ViewModels.MapSidebarViewModel)DataContext;
            m_viewModel.PropertyChanged += m_viewModel_PropertyChanged;

#if DEBUG
            Button testButton = new Button();
            testButton.Content = "Test";
            testButton.Click += TestButton_Click;
            ExpandedStackPanel.Children.Add(testButton);
#endif
        }

        void m_viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsExpanded":
                    // This state can be changed in other ways than MenuButton_Click.
                    SetMenuButtonState(m_viewModel.IsExpanded);
                    break;
            }
        }

        private ViewModels.MapSidebarViewModel m_viewModel;

        private void ExpandedScrollViewer_LostFocus(object sender, RoutedEventArgs e)
        {
            // This is a bit of a hack to get the expanded panel to go away when focus isn't inside it.
            // The "correct" way to do this would be to set FocusManager.FocusedElement="ExpandedScrollViewer" on all the stuff in it,
            // and then if this ever gets called, retract the expanded panel.
            // Silverlight doesn't implement the FocusManager.FocusedElement property, so we have to grab the focused element and
            // figure out whether it's a descendant of the expanded panel or not...

            FrameworkElement focused = System.Windows.Input.FocusManager.GetFocusedElement() as FrameworkElement;
            if (focused == null || (!MenuButton.HasDescendant(focused) && !ExpandedScrollViewer.HasDescendant(focused)))
            {
                // Note the exclusion of MenuButton above. If focus changed to the Menu button, there's a race condition between this
                // executing and its Click handler. If this function runs first, it will pop the menu back out again.

                m_viewModel.IsExpanded = false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_viewModel = (ViewModels.MapSidebarViewModel)DataContext;
        }

        private ViewModels.MapSidebarViewModel m_viewModel;
    }
}

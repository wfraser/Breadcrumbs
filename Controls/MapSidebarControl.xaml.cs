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
        static DependencyProperty IsGpsEnabledProperty = DependencyProperty.Register("IsGpsEnabled", typeof(bool), typeof(MapSidebarControl), new PropertyMetadata(new PropertyChangedCallback(OnIsGpsEnabledChanged)));
        static void OnIsGpsEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var sidebar = (MapSidebarControl)target;
            sidebar.GpsToggle.Text = "GPS " + ((bool)e.NewValue ? "Off" : "On");
            sidebar.GpsToggle.ImageSource = new Uri("/Assets/Icons/transport." + ((bool)e.NewValue ? "pause" : "play") + ".png", UriKind.Relative);
            sidebar.GpsToggle.IsEnabled = true;
        }
        public bool IsGpsEnabled
        {
            get { return (bool)GetValue(IsGpsEnabledProperty); }
            set { SetValue(IsGpsEnabledProperty, value); }
        }

        public MapSidebarControl()
        {
            InitializeComponent();
        }

        private void GpsToggle_Click(object sender, RoutedEventArgs e)
        {
            GpsToggle.IsEnabled = false;
            GpsToggle.Text = "...";
            m_viewModel.MainVM.ToggleGps();
        }

        private void TrackToggle_Click(object sender, RoutedEventArgs e)
        {
            TrackToggle.IsEnabled = false;
            TrackToggle.Text = "...";
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_viewModel = (ViewModels.MapSidebarViewModel)DataContext;
        }

        private ViewModels.MapSidebarViewModel m_viewModel;
    }
}

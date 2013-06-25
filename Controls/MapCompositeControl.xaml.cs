using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;

namespace DashMap
{
    public partial class MapCompositeControl : UserControl
    {
        const double MinZoomLevel = 1.0;
        const double MaxZoomLevel = 20.0;

        public MapCompositeControl()
        {
            InitializeComponent();
            m_mapLoaded = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_viewModel = (ViewModels.MapCompositeViewModel)DataContext;

            m_viewModel.MainVM.PropertyChanged += MainVM_PropertyChanged;
            m_viewModel.MainVM.TrackCleared += MainVM_TrackCleared;

            if (m_mapLoaded)
                m_viewModel.OverlayText = string.Empty;

            m_viewModel.GetCurrentLocation().ContinueWith(x =>
            {
                Windows.Devices.Geolocation.Geoposition location = x.Result;
                if (location == null)
                {
                    // Leave it at its default. (or set to (47.622344,-122.325865)?)
                }
                else
                {
                    m_viewModel.MainVM.CurrentPosition = location.Coordinate;
                    MapControl.Dispatcher.BeginInvoke(() =>
                        {
                            MapControl.ZoomLevel = 15;
                        });
                }
            });
        }

        void MainVM_TrackCleared()
        {
            Track.Path.Clear();
        }

        void MainVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // This is to work around the lack of dependency properties in most of the map controls.
            switch (e.PropertyName)
            {
                case "CurrentGeoCoordinate":
                    GeoCoordinate center = m_viewModel.MainVM.CurrentGeoCoordinate;
                    CurrentPositionCircle.Path = Utils.MakeCircle(center, center.HorizontalAccuracy);

                    if (m_viewModel.MainVM.IsTrackingEnabled)
                    {
                        Track.Path.Add(center);
                    }
                    break;
            }
        }

        private void MapControl_ViewChanging(object sender, MapViewChangingEventArgs e)
        {

        }

        private void MapControl_ViewChanged(object sender, MapViewChangedEventArgs e)
        {

        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            double zoom = MapControl.ZoomLevel;
            zoom += 1.0;
            zoom = Math.Min(Math.Max(zoom, MinZoomLevel), MaxZoomLevel);
            MapControl.ZoomLevel = zoom;

            ZoomOutButton.IsEnabled = true;
            if (zoom == MaxZoomLevel)
            {
                ZoomInButton.IsEnabled = false;
            }
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            double zoom = MapControl.ZoomLevel;
            zoom -= 1.0;
            zoom = Math.Min(Math.Max(zoom, MinZoomLevel), MaxZoomLevel);
            MapControl.ZoomLevel = zoom;

            ZoomInButton.IsEnabled = true;
            if (zoom == MinZoomLevel)
            {
                ZoomOutButton.IsEnabled = false;
            }
        }

        private void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_mapLoaded = true;
            if (m_viewModel != null)
                m_viewModel.OverlayText = string.Empty;

            Track = MapControl.MapElements[0] as MapPolyline;
            CurrentPositionCircle = MapControl.MapElements[1] as MapPolygon;
        }

        bool m_mapLoaded;
        private ViewModels.MapCompositeViewModel m_viewModel;
    }
}

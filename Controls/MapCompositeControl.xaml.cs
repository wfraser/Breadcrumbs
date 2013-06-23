using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;

namespace DashMap
{
    public partial class MapCompositeControl : UserControl
    {
        public MapCompositeControl()
        {
            InitializeComponent();
            m_mapLoaded = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_viewModel = (ViewModels.MapCompositeViewModel)DataContext;

            if (m_mapLoaded)
                m_viewModel.OverlayText = string.Empty;

            m_viewModel.GetCurrentLocation().ContinueWith(x =>
            {
                var location = x.Result;
                if (location == null)
                {
                    // Leave it at its default. (or set to (47.622344,-122.325865)?)
                }
                else
                {
                    MapControl.Dispatcher.BeginInvoke(new Action(() => {
                        MapControl.Center = Utils.ConvertGeocoordinate(location.Coordinate);
                        MapControl.ZoomLevel = 15;
                    }));
                }
            });
        }

        private void MapControl_ViewChanging(object sender, MapViewChangingEventArgs e)
        {

        }

        private void MapControl_ViewChanged(object sender, MapViewChangedEventArgs e)
        {

        }

        private void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_mapLoaded = true;
            if (m_viewModel != null)
                m_viewModel.OverlayText = string.Empty;
        }

        bool m_mapLoaded;
        private ViewModels.MapCompositeViewModel m_viewModel;
    }
}

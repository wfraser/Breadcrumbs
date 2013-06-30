using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DashMap.Resources;
using DashMap.ViewModels;

namespace DashMap
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            DataContext = m_viewModel = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                MessageBoxResult result = MessageBox.Show(
                    "This app accesses your phone's location. Is that okay?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            UriMapper uriMapper = (UriMapper)App.RootFrame.UriMapper;
            if (uriMapper.IncomingFileName != null)
            {
                MessageBox.Show(
                    "Sorry, support for loading GPX files isn't finished yet.",
                    "DashMap GPX Import",
                    MessageBoxButton.OK);
            }
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (LayoutRoot == null || !SystemTray.IsVisible)
                return;

            Thickness margin = LayoutRoot.Margin;
            switch (e.Orientation)
            {
                case PageOrientation.LandscapeLeft:
                    margin.Left = -70;
                    margin.Right = 0;
                    break;
                case PageOrientation.LandscapeRight:
                    margin.Left = 0;
                    margin.Right = -70;
                    break;
            }
            LayoutRoot.Margin = margin;
        }

        private void GpsToggle(object sender, EventArgs e)
        {
            m_viewModel.IsGpsEnabled ^= m_viewModel.IsGpsEnabled;
        }

        private MainViewModel m_viewModel;
    }
}
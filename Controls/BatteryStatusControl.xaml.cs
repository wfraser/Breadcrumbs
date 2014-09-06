using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Breadcrumbs
{
    public partial class BatteryStatusControl : UserControl
    {
        public BatteryStatusControl()
        {
            InitializeComponent();
            m_viewModel = new ViewModels.BatteryViewModel();
            m_viewModel.Start();
            DataContext = m_viewModel;
        }

        ~BatteryStatusControl()
        {
            m_viewModel.Stop();
        }

        ViewModels.BatteryViewModel m_viewModel;
    }
}

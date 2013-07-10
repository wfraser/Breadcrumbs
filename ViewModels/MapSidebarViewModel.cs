using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breadcrumbs.ViewModels
{
    public class MapSidebarViewModel : ViewModelBase
    {
        public MainViewModel MainVM
        {
            get { return m_mainVM; }
        }

        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                m_isExpanded = value;
                NotifyPropertyChanged("IsExpanded");
            }
        }

        public MapSidebarViewModel(MainViewModel mainVM)
        {
            m_mainVM = mainVM;
        }

        private bool m_isExpanded;
        private MainViewModel m_mainVM;
    }
}

using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
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

        public bool IsExtraExpanded
        {
            get { return m_isExtraExpanded; }
            set
            {
                m_isExtraExpanded = value;
                NotifyPropertyChanged("IsExtraExpanded");
            }
        }

        public MapSidebarViewModel(MainViewModel mainVM)
        {
            m_mainVM = mainVM;
        }

        private bool m_isExpanded;
        private bool m_isExtraExpanded;
        private MainViewModel m_mainVM;
    }
}

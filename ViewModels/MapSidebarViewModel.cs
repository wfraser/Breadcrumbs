using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashMap.ViewModels
{
    public class MapSidebarViewModel
    {
        public MainViewModel MainVM
        {
            get { return m_mainVM; }
        }

        public MapSidebarViewModel(MainViewModel mainVM)
        {
            m_mainVM = mainVM;
        }

        private MainViewModel m_mainVM;
    }
}

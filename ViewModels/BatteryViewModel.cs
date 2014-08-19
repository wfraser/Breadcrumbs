using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Phone.Info;
using Windows.Phone.Devices.Power;

namespace Breadcrumbs.ViewModels
{
    public class BatteryViewModel : ViewModelBase
    {
        public string ChargePercentage
        {
            get { return string.Format("{0}%", Battery.GetDefault().RemainingChargePercent); }
        }

        public string RemainingTime
        {
            get
            {
                TimeSpan t = Battery.GetDefault().RemainingDischargeTime;
                return string.Format("{0}:{1:D2}", Math.Floor(t.TotalHours), t.Minutes);
            }
        }

        public bool IsCharging
        {
            get { return DeviceStatus.PowerSource == PowerSource.External; }
        }

        public void Start()
        {
            Battery.GetDefault().RemainingChargePercentChanged += BatteryRemainingChargePercentChanged;
            DeviceStatus.PowerSourceChanged += DeviceStatus_PowerSourceChanged;
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromMinutes(1.0);
            m_timer.Tick += TimerTick;
            m_timer.Start();
        }

        public void Stop()
        {
            Battery.GetDefault().RemainingChargePercentChanged -= BatteryRemainingChargePercentChanged;
            DeviceStatus.PowerSourceChanged -= DeviceStatus_PowerSourceChanged;
            m_timer.Stop();
        }

        private void BatteryRemainingChargePercentChanged(object sender, object e)
        {
            NotifyPropertyChanged("ChargePercentage");
        }

        void DeviceStatus_PowerSourceChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged("IsCharging");
        }

        private void TimerTick(object sender, EventArgs e)
        {
            NotifyPropertyChanged("RemainingTime");
        }

        private DispatcherTimer m_timer;
    }
}

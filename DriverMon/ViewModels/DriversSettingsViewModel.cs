using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Zodiacon.WPF;

namespace DriverMon.ViewModels {
    sealed class DriversSettingsViewModel : DialogViewModelBase {
        Dictionary<string, DriverViewModel> _existingDrivers;

        public DriversSettingsViewModel(Window dialog, IEnumerable<DriverViewModel> existingDrivers) : base(dialog) {
            if (existingDrivers != null)
                _existingDrivers = existingDrivers.ToDictionary(driver => driver.Name);
        }

        List<DriverViewModel> _drivers;

        public List<DriverViewModel> Drivers {
            get {
                if (_drivers == null) {
                    var drivers = from svc in ServiceController.GetDevices()
                                  where svc.Status == ServiceControllerStatus.Running
                                  select svc;

                    _drivers = new List<DriverViewModel>(256);
                    foreach (var svc in drivers) {
                        if (_existingDrivers != null && _existingDrivers.TryGetValue(svc.ServiceName, out var existingDriver))
                            _drivers.Add(existingDriver);
                        else
                            _drivers.Add(new DriverViewModel(svc.ServiceName) {
                                DisplayName = svc.DisplayName,
                                IsMonitored = false
                            });
                    }
                }
                return _drivers;
            }
        }

    }
}

using Syncfusion.Data;
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
					_drivers = new List<DriverViewModel>(256);
					var directories = new string[] { @"\driver", @"\filesystem" };
					foreach (var dir in directories) {
						var drivers = Helpers.GetDriversFromObjectManager(dir);
						foreach (var name in drivers) {
							var displayName = string.Empty;
							try {
								using (var svc = new ServiceController(name)) {
									displayName = svc.DisplayName;
								}
							}
							catch (Exception) {
							}
							_drivers.Add(new DriverViewModel(name, dir) {
								DisplayName = displayName,
								IsMonitored = _existingDrivers != null && _existingDrivers.TryGetValue(name, out var vm) ? vm.IsMonitored : false
							});
						}
					}
                    _drivers.Sort((d1, d2) => d1.Name.CompareTo(d2.Name));
                }
                return _drivers;
            }
        }

        private string _filterText;

        public string FilterText {
            get => _filterText; 
            set {
                if (SetProperty(ref _filterText, value)) {
                    if (string.IsNullOrWhiteSpace(value)) {
                        View.Filter = null;
                    }
                    else {
                        var text = value.ToLowerInvariant();
                        View.Filter = obj => {
                            var vm = (DriverViewModel)obj;
                            return vm.Name.ToLowerInvariant().Contains(text) || vm.DisplayName.ToLowerInvariant().Contains(text);
                        };
                    }
                    View.RefreshFilter(true);
                }
            }
        }

        public ICollectionViewAdv View { get; set; }

    }
}

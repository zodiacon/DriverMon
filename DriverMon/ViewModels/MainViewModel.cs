using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Zodiacon.WPF;
using System.Windows;
using System.ServiceProcess;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DriverMon.Views;
using System.Windows.Data;
using Syncfusion.Data;
using MahApps.Metro;
using DriverMon.Models;

namespace DriverMon.ViewModels {
    class MainViewModel : BindableBase, IDisposable {
        DriverInterface _driver;
        public readonly IUIServices UI;
        readonly ObservableCollection<IrpViewModelBase> _requests = new ObservableCollection<IrpViewModelBase>();
        readonly Dictionary<IntPtr, DriverViewModel> _driversd = new Dictionary<IntPtr, DriverViewModel>(8);
        readonly Dictionary<IntPtr, IrpArrivedViewModel> _irps = new Dictionary<IntPtr, IrpArrivedViewModel>(64);

        List<DriverViewModel> _drivers;
        Settings _settings;

        public Settings Settings => _settings;
        public ObservableCollection<IrpViewModelBase> Requests => _requests;
        public IList<DriverViewModel> Drivers => _drivers;
        public int MonitoredDrivers => _driversd.Count;

        AccentViewModel[] _accents;
        public AccentViewModel[] Accents => _accents ?? (_accents = ThemeManager.Accents.Select(accent => new AccentViewModel(accent)).ToArray());
        AccentViewModel _currentAccent;
        public AccentViewModel CurrentAccent => _currentAccent;

        public ICommand ChangeAccentCommand => new DelegateCommand<AccentViewModel>(accent => {
            if (_currentAccent != null)
                _currentAccent.IsCurrent = false;
            _currentAccent = accent;
            Settings.AccentColor = accent.Name;
            accent.IsCurrent = true;
            RaisePropertyChanged(nameof(CurrentAccent));
        }, accent => accent != _currentAccent).ObservesProperty(() => CurrentAccent);

        public bool IsAlwaysOnTop {
            get => Application.Current.MainWindow.Topmost;
            set => Application.Current.MainWindow.Topmost = Settings.Topmost = value;
        }

        public MainViewModel(IUIServices ui) {
            UI = ui;
        }

        public ICommand OnLoad => new DelegateCommand(async () => {
            LoadSettings();
            IsAlwaysOnTop = Settings.Topmost;
            ChangeAccentCommand.Execute(Accents.First(acc => acc.Name == _settings.AccentColor));

            await StartDriver();
            _driver.RemoveAllDrivers();
        });

        private async Task InstallAndLoadDriverAsync() {
            var status = await DriverInterface.LoadDriverAsync("DriverMon");
            if (status == null) {
                var ok = await DriverInterface.InstallDriverAsync("DriverMon", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\DriverMonitor.sys");
                if (!ok) {
                    UI.MessageBoxService.ShowMessage("Failed to install driver. Exiting", App.Title);
                    Application.Current.Shutdown(1);
                }
                status = await DriverInterface.LoadDriverAsync("DriverMon");
            }
            if (status != ServiceControllerStatus.Running) {
                UI.MessageBoxService.ShowMessage("Failed to start driver. Exiting", App.Title);
                Application.Current.Shutdown(1);
            }
        }

        private async Task StartDriver() {
            if (_driver != null)
                return;

            try {
                _driver = new DriverInterface();
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 2) {
                await InstallAndLoadDriverAsync();
                _driver = new DriverInterface();
            }
            catch (Exception ex) {
                UI.MessageBoxService.ShowMessage($"Error: {ex.Message}", App.Title);
                Application.Current.Shutdown(1);
            }
        }

        public void Dispose() {
            _driver.Dispose();
        }

        public unsafe void Update(byte[] data, int size) {
            fixed (byte* bytes = data) {
                var p = bytes;
                do {
                    var info = (CommonInfoHeader*)p;
                    Debug.Assert(info->Size > sizeof(CommonInfoHeader));

                    switch (info->Type) {
                        case DataItemType.IrpArrived:
                            var arrivedInfo = (IrpArrivedInfoBase*)info;
                            var request = new IrpArrivedViewModel(_requests.Count + 1, _driversd[arrivedInfo->DriverObject].Name, arrivedInfo);
                            _requests.Add(request);
                            _irps[arrivedInfo->Irp] = request;
                            break;

                        case DataItemType.IrpCompleted:
                            var completedInfo = (IrpCompletedInfo*)info;
                            bool exist = _irps.TryGetValue(completedInfo->Irp, out var requestData);
                            _requests.Add(new IrpCompletedViewModel(_requests.Count + 1, completedInfo, requestData));
                            if (exist) {
                                _irps.Remove(completedInfo->Irp);
                            }
                            break;

                        default:
                            Debug.Assert(false);
                            break;
                    }
                    size -= info->Size;
                    p += info->Size;
                } while (size > sizeof(CommonInfoHeader));
            }
            RaisePropertyChanged(nameof(FilteredCount));
        }

        public ICollectionViewAdv View { get; set; }

        public ICommand DriversSettingsCommand => new DelegateCommand(() => {
            var vm = UI.DialogService.CreateDialog<DriversSettingsViewModel, DriversSettingsDialog>(Drivers);
            if (vm.ShowDialog() == true) {
                _drivers = vm.Drivers;
                _driver.RemoveAllDrivers();
                _driversd.Clear();

                foreach (var driver in _drivers.Where(d => d.IsMonitored)) {
                    string driverName = driver.Directory + "\\" + driver.Name;
                    var result = _driver.AddDriver(driverName);
                    if (result == IntPtr.Zero) {
                        UI.MessageBoxService.ShowMessage($"Failed to hook driver {driverName}", App.Title);
                    }
                    else {
                        _driversd.Add(result, driver);
                    }
                }
                RaisePropertyChanged(nameof(MonitoredDrivers));
            }
        });

        bool _isMonitoring;
        public bool IsMonitoring {
            get => _isMonitoring;
            set {
                if (SetProperty(ref _isMonitoring, value)) {
                    if (value) {
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                        _driver.StartMonitoring();
                    }
                    else {
                        _driver.StopMonitoring();
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
                    }
                }
            }
        }

        bool _autoScroll;
        public bool AutoScroll {
            get => _autoScroll;
            set => SetProperty(ref _autoScroll, value);
        }

        string _searchText;
        public string SearchText {
            get => _searchText;
            set {
                if (SetProperty(ref _searchText, value)) {
                    if (string.IsNullOrWhiteSpace(value)) {
                        View.Filter = null;
                    }
                    else {
                        var text = value.ToLowerInvariant();
                        View.Filter = obj => {
                            var request = (IrpViewModelBase)obj;
                            return request.DriverName.ToLowerInvariant().Contains(text) || request.Function.ToString().ToLowerInvariant().Contains(text)
                                || request.ProcessName?.ToLowerInvariant().Contains(text) == true;
                        };
                    }
                    View.RefreshFilter(true);
                    RaisePropertyChanged(nameof(FilteredCount));
                }
            }
        }

        public ICommand ClearAllCommand => new DelegateCommand(() => {
            Requests.Clear();
        });

        public ICommand ViewDataCommand => new DelegateCommand<IrpViewModelBase>(request => {
            Debug.Assert(request.DataSize > 0);
            Debug.Assert(request.Data != null);

            var vm = UI.DialogService.CreateDialog<DataBufferViewModel, DataBufferDialog>(request);
            vm.Show();
        });

        public int FilteredCount => View.Records.Count;

        private void LoadSettings() {
            try {
                using (var stm = File.OpenRead(GetSettingsFile())) {
                    _settings = Helpers.Load<Settings>(stm);
                }
            }
            catch (Exception) {
            }

            if (_settings == null)
                _settings = new Settings();
        }

        string GetSettingsFile() {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\DriverMon.settings.xml";
        }

        public void Save() {
            var filename = GetSettingsFile();
            if (File.Exists(filename))
                File.Delete(filename);
            using (var stm = File.OpenWrite(filename)) {
                Helpers.Save(stm, _settings);
            }
        }

    }
}

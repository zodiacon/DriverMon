using MahApps.Metro;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DriverMon.ViewModels {
    class AccentViewModel : BindableBase {
        Accent _accent;
        public AccentViewModel(Accent accent) {
            _accent = accent;
        }

        public string Name => _accent.Name;
        public Brush Brush => _accent.Resources["AccentColorBrush"] as Brush;

        private bool _isCurrent;

        public bool IsCurrent {
            get { return _isCurrent; }
            set {
                if (SetProperty(ref _isCurrent, value) && value) {
                    ChangeAccentColor();
                }
            }
        }

        public void ChangeAccentColor() {
            ThemeManager.ChangeAppStyle(Application.Current, _accent, ThemeManager.DetectAppStyle().Item1);
        }
    }
}

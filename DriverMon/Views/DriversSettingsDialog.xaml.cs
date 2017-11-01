using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DriverMon.Views {
    /// <summary>
    /// Interaction logic for DriversSettingsDialog.xaml
    /// </summary>
    public partial class DriversSettingsDialog {
        public DriversSettingsDialog() {
            InitializeComponent();

            Topmost = Application.Current.MainWindow.Topmost;
        }

        private void OnGridLoaded(object sender, RoutedEventArgs e) {
            SfSkinManager.SetVisualStyle((DependencyObject)sender, VisualStyles.Metro);
        }
    }
}

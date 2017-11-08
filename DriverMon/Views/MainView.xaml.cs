using DriverMon.ViewModels;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DriverMon.Views {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl {
        public MainView() {
            InitializeComponent();
        }

        private void OnGridLoaded(object sender, RoutedEventArgs e) {
            SfSkinManager.SetVisualStyle((DependencyObject)sender, VisualStyles.Metro);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var vm = e.NewValue as MainViewModel;
            if (vm != null) {
                vm.Requests.CollectionChanged += delegate {
                    if (vm.AutoScroll && vm.Requests.Count > 0)
                        _dataGrid.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(vm.Requests.Count - 1, 0));
                };
            }
        }
    }
}

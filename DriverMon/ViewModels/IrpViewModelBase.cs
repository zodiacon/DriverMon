using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverMon.ViewModels {
    enum IrpType {
        Sent,
        CompleteSuccess,
        CompleteError,
        Cancelled
    }

    abstract class IrpViewModelBase {
        unsafe public IrpViewModelBase(int index, string driverName, CommonInfoHeader* info) {
            Index = index;
            Time = new DateTime(info->Time);
            DriverName = driverName;
        }

        public IrpType IrpType { get; protected set; }
        public string Icon { get; protected set; }

        public int Index { get; }
        public DateTime Time { get; }
        public string DriverName { get; }
    }
}

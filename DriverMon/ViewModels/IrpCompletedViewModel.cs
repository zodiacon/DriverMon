using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverMon.ViewModels {
    class IrpCompletedViewModel : IrpViewModelBase {
        IrpArrivedViewModel _arrived;
        unsafe public IrpCompletedViewModel(int index, IrpCompletedInfo* info, IrpArrivedViewModel arrived) : base(index, arrived.DriverName, &info->Header) {
            _arrived = arrived;
            Irp = info->Irp.ToInt64();
            Status = info->Status;
            Information = info->Information.ToInt64();
            Details = $"Status: 0x{Status:X}; Information=0x{Information:X}";
            DeviceObject = arrived.DeviceObject;
        }

        public long Irp { get; }
        public long DeviceObject { get; }
        public int Status { get; }
        public long Information;
        public string Details { get; }
    }
}

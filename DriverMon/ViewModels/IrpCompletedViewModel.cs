using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverMon.ViewModels {
    class IrpCompletedViewModel : IrpViewModelBase {
        unsafe public IrpCompletedViewModel(int index, string driverName, IrpCompletedInfo* info) : base(index, driverName, &info->Header) {
            DriverObject = info->DriverObject.ToInt64();
            DeviceObject = info->DeviceObject.ToInt64();
            Irp = info->Irp.ToInt64();
            Status = info->Status;
            Information = info->Information.ToInt64();
            Details = $"Status: 0x{Status:X}; Information=0x{Information:X}";
        }

        public long Irp { get; }
        public long DriverObject { get; }
        public long DeviceObject { get; }
        public int Status { get; }
        public long Information;
        public string Details { get; }
    }
}

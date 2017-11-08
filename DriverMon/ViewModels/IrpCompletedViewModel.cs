using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverMon.ViewModels {
    class IrpCompletedViewModel : IrpViewModelBase {
        const uint StatusCancelled = 0xC0000120;

        IrpArrivedViewModel _arrived;
        unsafe public IrpCompletedViewModel(int index, IrpCompletedInfo* info, IrpArrivedViewModel arrived) : base(index, arrived.DriverName, &info->Header) {
            _arrived = arrived;
            Status = info->Status;
            Information = info->Information.ToInt64();
            Details = $"Status: 0x{Status:X}; Information=0x{Information:X}";

            if (Status >= 0) {
                IrpType = IrpType.CompleteSuccess;
                Icon = "/icons/irp-success.ico";
            }
            else if ((uint)Status == StatusCancelled) {
                IrpType = IrpType.Cancelled;
                Icon = "/icons/irp-cancel.ico";
            }
            else {
                IrpType = IrpType.CompleteError;
                Icon = "/icons/irp-error.ico";
            }
        }

        public string Function => _arrived.Function;
        public long Irp => _arrived.Irp;
        public long DeviceObject => _arrived.DeviceObject;
        public int Status { get; }
        public long Information { get; }
        public string Details { get; }
    }
}

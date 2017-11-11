using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverMon.ViewModels {
    class IrpCompletedViewModel : IrpViewModelBase {
        const uint StatusCancelled = 0xC0000120;

        IrpArrivedViewModel _arrived;
        unsafe public IrpCompletedViewModel(int index, IrpCompletedInfo* info, IrpArrivedViewModel arrived) : base(index, arrived?.DriverName, &info->Header) {
            _arrived = arrived;
            Status = info->Status;
            Information = info->Information.ToInt64();
            Details = $"Status: 0x{Status:X}; Information=0x{Information:X}";

            if (Status == 0x103) {  // STATUS_PENDING
                IrpType = IrpType.Pending;
                Icon = "/icons/clock.ico";
            }
            else if (Status >= 0) {
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

            Irp = _arrived != null ? _arrived.Irp : 0;
            MajorCode = _arrived != null ? _arrived.MajorCode : IrpMajorCode.UNKNOWN;
            DataSize = info->DataSize;

            if (DataSize > 0) {
                Data = new byte[DataSize];
                fixed (byte* p = Data) {
                    Buffer.MemoryCopy((byte*)info + info->Header.Size - DataSize, p, DataSize, DataSize);
                }
            }
            Function = _arrived?.Function;
        }

        
        public long DeviceObject => _arrived != null ? _arrived.DeviceObject : 0;
        public int Status { get; }
        public long Information { get; }
        public string Details { get; }
    }
}

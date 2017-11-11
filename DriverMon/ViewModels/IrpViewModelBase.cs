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
        Cancelled,
        Pending
    }

    abstract class IrpViewModelBase {
        unsafe public IrpViewModelBase(int index, string driverName, CommonInfoHeader* info) {
            Index = index;
            Time = new DateTime(info->Time);
            DriverName = driverName;
            ProcessId = info->ProcessId;
            ThreadId = info->ThreadId;
        }

        public int ProcessId { get; }
        public int ThreadId { get; }
        public long Irp { get; protected set; }
        public IrpType IrpType { get; protected set; }
        public string Icon { get; protected set; }
        public IrpMajorCode MajorCode { get; protected set; }
        public int Index { get; }
        public DateTime Time { get; }
        public string DriverName { get; }
        public int DataSize { get; protected set; }
        public byte[] Data { get; protected set; }
        public bool HasData => DataSize > 0;
        public string ProcessName { get; protected set; }
        public string Function { get; protected set; }
    }
}

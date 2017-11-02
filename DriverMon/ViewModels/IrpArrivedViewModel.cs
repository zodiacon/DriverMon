using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DriverMon.Win32;

namespace DriverMon.ViewModels {
    class IrpArrivedViewModel {
        static StringBuilder _sb = new StringBuilder(260);

        unsafe public IrpArrivedViewModel(int index, string driverName, IrpArrivedInfoBase* info) {
            Index = index;
            Time = new DateTime(info->Header.Time);
            ProcessId = info->ProcessId;
            ThreadId = info->ThreadId;
            MajorCode = info->MajorFunction;
            switch (MajorCode) {
                case IrpMajorCode.PNP:
                    MinorCode = ((IrpMinorCodePnp)info->MinorFunction).ToString();
                    break;

                case IrpMajorCode.POWER:
                    MinorCode = ((IrpMinorCodePower)info->MinorFunction).ToString();
                    break;

                case IrpMajorCode.SYSTEM_CONTROL:
                    MinorCode = ((IrpMinorCodeWmi)info->MinorFunction).ToString();
                    break;

                default:
                    MinorCode = ((int)info->MinorFunction).ToString();
                    break;
            }

            DriverObject = info->DriverObject.ToInt64();
            DeviceObject = info->DeviceObject.ToInt64();
            DriverName = driverName;
            Irp = info->Irp;
            if (ProcessId == 4) {
                ProcessName = "System";
            }
            else {
                using (var process = OpenProcess(ProcessAccessMask.QueryLimitedInformation, false, ProcessId)) {
                    if (!process.IsInvalid) {
                        int size = _sb.Capacity;
                        if (QueryFullProcessImageName(process, ImageNameType.Normal, _sb, ref size))
                            ProcessName = Path.GetFileName(_sb.ToString());
                    }
                }
            }

            Details = GetDetails(info);
        }

        private unsafe string GetDetails(IrpArrivedInfoBase* info) {
            switch (info->MajorFunction) {
                case IrpMajorCode.READ:
                case IrpMajorCode.WRITE:
                    var read = (IrpArrivedInfoReadWrite*)info;
                    return $"Offset: 0x{read->Offset:X}; Length: 0x{read->Length:X}";

                case IrpMajorCode.DEVICE_CONTROL:
                case IrpMajorCode.INTERNAL_DEVICE_CONTROL:
                    var dc = (IrpArrivedInfoDeviceIoControl*)info;
                    return $"Ioctl: 0x{dc->IoControlCode:X}; Input: {dc->InputBufferLength} bytes; Output: {dc->OutputBufferLength} bytes";

                default:
                    return string.Empty;
            }
        }

        public string ProcessName { get; }
        public int Index { get; }
        public DateTime Time { get; }
        public int ProcessId { get; }
        public int ThreadId { get; }
        public IrpMajorCode MajorCode { get; }
        public string MinorCode { get; }
        public long DriverObject { get; }
        public long DeviceObject { get; }
        public IntPtr Irp { get; }
        public string DriverName { get; }
        public string Details { get; }
    }
}

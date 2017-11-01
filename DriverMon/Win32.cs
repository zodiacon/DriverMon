using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DriverMon {

    [SuppressUnmanagedCodeSecurity]
    static class Win32 {
        [Flags]
        public enum FileShareMode {
            None = 0,
            Read = 1,
        }

        public enum CreationDisposition {
            OpenExisting = 3
        }

        public enum CreateFileFlags {
            None = 0,
            Overlapped = 0x40000000
        }

        [Flags]
        public enum FileAccessMask : uint {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000
        }

        public enum ServiceType {
            KernelDriver = 1
        }
        public enum ServiceStartType {
            DemandStart = 3
        }
        public enum ServiceErrorControl {
            Normal = 1
        }

        [Flags]
        public enum ServiceAccessMask {
            Connect = 0x0001,
            CreateService = 0x0002,
            EnumerateService = 0x0004,
            Lock = 0x0008,
            LockStatus = 0x0010,
            ModifyBootConfig = 0x0020,
            AllAccess = 0xf0000 | Connect | CreateService | EnumerateService | Lock | LockStatus | ModifyBootConfig
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public unsafe static extern bool DeviceIoControl(SafeFileHandle hDevice, int controlCode, void* address, int inputSize,
                byte[] buffer, int outputSize, out int returned, NativeOverlapped* overlapped = null);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public unsafe static extern bool DeviceIoControl(SafeFileHandle hDevice, int controlCode, string text, int inputSize,
                out IntPtr result, int outputSize, out int returned, NativeOverlapped* overlapped = null); 

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public unsafe static extern bool DeviceIoControl(SafeFileHandle hDevice, int controlCode, void* address, int inputSize,
                out IntPtr buffer, int outputSize, out int returned, NativeOverlapped* overlapped = null);


        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string path, FileAccessMask accessMask, FileShareMode shareMode,
            IntPtr sd, CreationDisposition disposition, CreateFileFlags flags, IntPtr hTemplateFile);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, ServiceAccessMask accessMask);
        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hScm, string serviceName, ServiceAccessMask accessMask);
        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool DeleteService(IntPtr hService);
        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateService(IntPtr hScm, string serviceName, string displayName, ServiceAccessMask desiredAccess,
            ServiceType serviceType, ServiceStartType startType, ServiceErrorControl errorControl,
            string imagePath, string loadOrderGroup, IntPtr tag,
            string dependencies = null, string serviceStartName = null, string password = null);
        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr handle);

        public enum ProcessAccessMask : uint {
            QueryLimitedInformation = 0x1000,
        }

        public enum ImageNameType {
            Normal,
            Native = 1
        }

        [DllImport("kernel32")]
        public static extern SafeWaitHandle OpenProcess(ProcessAccessMask accessMask, bool inheritHandle, int pid);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool QueryFullProcessImageName(SafeWaitHandle handle, ImageNameType type, StringBuilder imagePath, ref int size);

    }
}

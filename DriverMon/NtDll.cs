using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DriverMon {
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct UnicodeString {
        public short Length;
        public short MaximumLength;
        public char* Buffer;

        unsafe public void Init(char* name, int len) {
            Buffer = name;
            Length = MaximumLength = (short)(len * 2);
        }
    }

    [Flags]
    enum ObjectAttributesFlags : uint {
        CaseInsensitive = 0x40
    }

    [Flags]
    enum DirectoryAccessMask : uint {
        Query = 1,
        Traverse = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct ObjectAttributes {
        int Length;
        IntPtr RootDirectory;
        void* ObjectName;
        ObjectAttributesFlags Attributes;
        IntPtr SecurityDescriptor;        // Points to type SECURITY_DESCRIPTOR
        IntPtr SecurityQualityOfService;  // Points to type SECURITY_QUALITY_OF_SERVICE

        public void Init(void* str, ObjectAttributesFlags flags) {
            Length = sizeof(ObjectAttributes);
            ObjectName = str;
            Attributes = flags;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ObjectDirectoryInformation {
        public UnicodeString Name;
        public UnicodeString TypeName;
    }

    [SuppressUnmanagedCodeSecurity]
    static class NtDll {
        [DllImport("ntdll")]
        public static extern int NtOpenDirectoryObject(out SafeWaitHandle hDirectory, DirectoryAccessMask accessMask, ref ObjectAttributes attributes);

        [DllImport("ntdll")]
        public unsafe static extern int NtQueryDirectoryObject(SafeWaitHandle hDirectory, ObjectDirectoryInformation* info,
            int bufferSize, bool onlyFirstEntry, bool firstEntry, ref int entryIndex, out int bytesReturned);

    }
}

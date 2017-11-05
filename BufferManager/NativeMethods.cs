using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	[Flags]
	enum ProcessAccessMask {
		None = 0,
		VmRead = 0x10,
		VmWrite = 0x20,
		VmOperation = 0x08,
		QueryInformation = 0x400,
	}

	[SuppressUnmanagedCodeSecurity]
	static class NativeMethods {
		[DllImport("kernel32", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessMask accessMask, bool inheritHandle, int pid);

		[DllImport("kernel32", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr handle);
	}
}

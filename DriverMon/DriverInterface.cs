using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using static DriverMon.Win32;

#pragma warning disable 649

namespace DriverMon {
	enum IrpMajorCode : byte {
		CREATE = 0x00,
		CREATE_NAMED_PIPE = 0x01,
		CLOSE = 0x02,
		READ = 0x03,
		WRITE = 0x04,
		QUERY_INFORMATION = 0x05,
		SET_INFORMATION = 0x06,
		QUERY_EA = 0x07,
		SET_EA = 0x08,
		FLUSH_BUFFERS = 0x09,
		QUERY_VOLUME_INFORMATION = 0x0a,
		SET_VOLUME_INFORMATION = 0x0b,
		DIRECTORY_CONTROL = 0x0c,
		FILE_SYSTEM_CONTROL = 0x0d,
		DEVICE_CONTROL = 0x0e,
		INTERNAL_DEVICE_CONTROL = 0x0f,
		SHUTDOWN = 0x10,
		LOCK_CONTROL = 0x11,
		CLEANUP = 0x12,
		CREATE_MAILSLOT = 0x13,
		QUERY_SECURITY = 0x14,
		SET_SECURITY = 0x15,
		POWER = 0x16,
		SYSTEM_CONTROL = 0x17,
		DEVICE_CHANGE = 0x18,
		QUERY_QUOTA = 0x19,
		SET_QUOTA = 0x1a,
		PNP = 0x1b,

		UNKNOWN = 0xff
	}

	enum IrpMinorCodePnp : byte {
		START_DEVICE = 0x00,
		QUERY_REMOVE_DEVICE = 0x01,
		REMOVE_DEVICE = 0x02,
		CANCEL_REMOVE_DEVICE = 0x03,
		STOP_DEVICE = 0x04,
		QUERY_STOP_DEVICE = 0x05,
		CANCEL_STOP_DEVICE = 0x06,

		QUERY_DEVICE_RELATIONS = 0x07,
		QUERY_INTERFACE = 0x08,
		QUERY_CAPABILITIES = 0x09,
		QUERY_RESOURCES = 0x0A,
		QUERY_RESOURCE_REQUIREMENTS = 0x0B,
		QUERY_DEVICE_TEXT = 0x0C,
		FILTER_RESOURCE_REQUIREMENTS = 0x0D,

		READ_CONFIG = 0x0F,
		WRITE_CONFIG = 0x10,
		EJECT = 0x11,
		SET_LOCK = 0x12,
		QUERY_ID = 0x13,
		QUERY_PNP_DEVICE_STATE = 0x14,
		QUERY_BUS_INFORMATION = 0x15,
		DEVICE_USAGE_NOTIFICATION = 0x16,
		SURPRISE_REMOVAL = 0x17,
	}

	enum IrpMinorCodePower {
		WAIT_WAKE = 0x00,
		POWER_SEQUENCE = 0x01,
		SET_POWER = 0x02,
		QUERY_POWER = 0x03,
	}

	enum IrpMinorCodeWmi {
		QUERY_ALL_DATA = 0x00,
		QUERY_SINGLE_INSTANCE = 0x01,
		CHANGE_SINGLE_INSTANCE = 0x02,
		CHANGE_SINGLE_ITEM = 0x03,
		ENABLE_EVENTS = 0x04,
		DISABLE_EVENTS = 0x05,
		ENABLE_COLLECTION = 0x06,
		DISABLE_COLLECTION = 0x07,
		REGINFO = 0x08,
		EXECUTE_METHOD = 0x09,
	}

	enum DataItemType : short {
		IrpArrived,
		IrpCompleted,
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct CommonInfoHeader {
		public short Size;
		public DataItemType Type;
		public long Time;
		public int ProcessId;
		public int ThreadId;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct IrpArrivedInfoBase {
		public CommonInfoHeader Header;

		public IntPtr DeviceObject;
		public IntPtr DriverObject;
		public IntPtr Irp;
		public IrpMajorCode MajorFunction;
		public byte MinorFunction;
		public byte Irql;
		byte _padding;
		public int DataSize;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct IrpArrivedInfoReadWrite {
		public IrpArrivedInfoBase Base;
		public uint Length;
		public long Offset;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct IrpArrivedInfoDeviceIoControl {
		public IrpArrivedInfoBase Base;
		public uint IoControlCode;
		public uint InputBufferLength;
		public uint OutputBufferLength;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct IrpCompletedInfo {
		public CommonInfoHeader Header;
		public IntPtr Irp;
		public int Status;
		public IntPtr Information;
		public int DataSize;
	}

	sealed class DriverInterface : IDisposable {
		const int DeviceType = 0x22;
		const int MethodBufferred = 0;
		const int MethodInDirect = 1;
		const int MethodOutDirect = 2;
		const int MethodNeither = 3;
		const int FileReadAccess = 1;
		const int FileWriteAccess = 2;
		const int FileAnyAccess = 0;
		static int ControlCode(int DeviceType, int Function, int Method, int Access) =>
			(DeviceType << 16) | (Access << 14) | (Function << 2) | Method;

		public static readonly int IoctlStartMonitoring = ControlCode(DeviceType, 0x801, MethodNeither, FileAnyAccess);
		public static readonly int IoctlStopMonitoring = ControlCode(DeviceType, 0x802, MethodNeither, FileAnyAccess);
		public static readonly int IoctlAddDriver = ControlCode(DeviceType, 0x810, MethodBufferred, FileAnyAccess);
		public static readonly int IoctlRemoveDriver = ControlCode(DeviceType, 0x811, MethodBufferred, FileAnyAccess);
		public static readonly int IoctlRemoveAll = ControlCode(DeviceType, 0x812, MethodNeither, FileAnyAccess);
		public static readonly int IoctlSetEventHandle = ControlCode(DeviceType, 0x820, MethodBufferred, FileAnyAccess);
		public static readonly int IoctlGetData = ControlCode(DeviceType, 0x821, MethodOutDirect, FileAnyAccess);

		SafeFileHandle _handle;
		const string deviceName = @"\\.\DriverMon";
		AutoResetEvent _event = new AutoResetEvent(false);
		ManualResetEvent _stopEvent = new ManualResetEvent(false);

		unsafe public DriverInterface() {
			_handle = CreateFile(deviceName, FileAccessMask.GenericRead | FileAccessMask.GenericWrite, FileShareMode.Read,
				IntPtr.Zero, CreationDisposition.OpenExisting, CreateFileFlags.None, IntPtr.Zero);
			if (_handle.IsInvalid) {
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			var rawHandle = _event.SafeWaitHandle.DangerousGetHandle();
			DeviceIoControl(_handle, IoctlSetEventHandle, &rawHandle, IntPtr.Size, null, 0, out var _);
		}

		public void Dispose() {
			StopDriver();
			_handle.Dispose();
		}

		private void StopDriver() {
			try {
				StopMonitoring();
				var svc = ServiceController.GetDevices().First(dev => dev.ServiceName.Equals("DriverMon", StringComparison.InvariantCultureIgnoreCase));
				svc.Stop();
			}
			catch {
			}
		}

		public static async Task<bool> InstallDriverAsync(string drivername, string driverpath) {
			await Task.Run(() => {
				var hScm = OpenSCManager(null, null, ServiceAccessMask.AllAccess);
				if (hScm == IntPtr.Zero)
					throw new Win32Exception(Marshal.GetLastWin32Error());

				// if driver exists, delete it first
				var hService = OpenService(hScm, drivername, ServiceAccessMask.AllAccess);
				if (hService != IntPtr.Zero) {
					// delete service
					DeleteService(hService);
					CloseServiceHandle(hService);
				}

				hService = CreateService(hScm, drivername, drivername, ServiceAccessMask.AllAccess,
					Win32.ServiceType.KernelDriver, ServiceStartType.DemandStart, ServiceErrorControl.Normal,
					driverpath, null, IntPtr.Zero);
				CloseServiceHandle(hScm);

				if (hService == IntPtr.Zero)
					throw new Win32Exception(Marshal.GetLastWin32Error());
				CloseServiceHandle(hService);
			});

			return true;
		}

		public static async Task<ServiceControllerStatus?> LoadDriverAsync(string drivername) {
			try {
				var controller = new ServiceController(drivername);

				if (controller.Status == ServiceControllerStatus.Running)
					return controller.Status;

				controller.Start();
				await Task.Run(() => controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5)));
				return controller.Status;
			}
			catch (Exception) {
				return null;
			}
		}

		public unsafe IntPtr AddDriver(string name) {
			if (DeviceIoControl(_handle, IoctlAddDriver, name, name.Length * 2 + 2, out var driver, IntPtr.Size, out var returned))
				return driver;
			return IntPtr.Zero;
		}

		public unsafe bool RemoveDriver(string name) {
			fixed (char* sname = name) {
				return DeviceIoControl(_handle, IoctlRemoveDriver, sname, name.Length * 2 + 2, null, 0, out var returned);
			}
		}

		public unsafe bool StartMonitoring() {
			var dispatcher = Dispatcher.CurrentDispatcher;
			if (!DeviceIoControl(_handle, IoctlStartMonitoring, null, 0, null, 0, out var returned))
				return false;

			var th = new Thread(() => {
				var handles = new WaitHandle[] { _event, _stopEvent };
				for (; ; ) {
					int index = WaitHandle.WaitAny(handles);
					if (index == 1)
						break;

					var data = GetData(out var size);
					if (data != null) {
						dispatcher.Invoke(() => App.MainViewModel.Update(data, size), DispatcherPriority.Background);
					}
				}
			});
			th.Priority = ThreadPriority.BelowNormal;
			th.IsBackground = true;
			th.Start();
			return true;
		}

		public unsafe bool StopMonitoring() {
			return DeviceIoControl(_handle, IoctlStopMonitoring, null, 0, null, 0, out var returned);
		}

		public unsafe bool RemoveAllDrivers() {
			return DeviceIoControl(_handle, IoctlRemoveAll, null, 0, null, 0, out var returned);
		}

		byte[] _buffer = new byte[1 << 20];

		public unsafe byte[] GetData(out int size) {
			if (DeviceIoControl(_handle, IoctlGetData, null, 0, _buffer, _buffer.Length, out size))
				return _buffer;
			return null;
		}
	}
}

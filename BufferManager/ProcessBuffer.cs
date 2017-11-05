using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	sealed class ProcessBuffer : BufferBase, IBufferOperations {
		readonly IntPtr _hProcess;
		public ProcessBuffer(int pid) {
			var flags = ProcessAccessMask.QueryInformation | ProcessAccessMask.VmRead | ProcessAccessMask.VmOperation;
			_hProcess = NativeMethods.OpenProcess(flags | ProcessAccessMask.VmWrite, false, pid);
			if(_hProcess == IntPtr.Zero)
				_hProcess = NativeMethods.OpenProcess(flags, false, pid);
			if (_hProcess == IntPtr.Zero)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		public override long Size => throw new NotImplementedException();

		public override void Delete(long offset, int count) {
			throw new NotImplementedException();
		}

		public override byte[] GetBytes(long offset, ref int count) {
			throw new NotImplementedException();
		}

		public override void Insert(long offset, byte[] data) {
			throw new NotImplementedException();
		}

		public override void SetData(long offset, byte[] data) {
			throw new NotImplementedException();
		}

		public override void Dispose() {
			NativeMethods.CloseHandle(_hProcess);
		}

		public void InsertData(long offset, byte[] data) {
			throw new NotImplementedException();
		}

		public void DeleteRange(long offset, int count) {
			throw new NotImplementedException();
		}

		public void SetBytes(long offset, byte[] data) {
			throw new NotImplementedException();
		}
	}
}

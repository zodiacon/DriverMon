using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager.Commands {
	class SetDataCommand : ICommand {
		readonly IBufferManager _buffer;
		readonly IBufferOperations _operations;
		readonly long _offset;
		byte[] _data;

		public SetDataCommand(IBufferManager buffer, long offset, byte[] data) {
			_buffer = buffer;
			_offset = offset;
			_data = data;
			_operations = (IBufferOperations)buffer;
			if (_operations == null)
				throw new ArgumentException(nameof(buffer));
		}

		public void Execute() {
			int count = _data.Length;
			var bytes = _buffer.GetBytes(_offset, ref count);
			_operations.SetBytes(_offset, _data);
			_data = bytes;
		}

		public void Undo() {
			Execute();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager.Commands {
	class InsertCommand : ICommand {
		protected readonly IBufferOperations _buffer;
		protected readonly long _offset;
		protected readonly byte[] _data;

		public InsertCommand(IBufferOperations buffer, long offset, byte[] data) {
			_buffer = buffer;
			_offset = offset;
			_data = data;
		}

		public virtual void Execute() {
			_buffer.InsertData(_offset, _data);
		}

		public virtual void Undo() {
			_buffer.DeleteRange(_offset, _data.Length);
		}
	}
}

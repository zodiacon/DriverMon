using BufferManager.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	sealed class ByteBuffer : BufferBase, IBufferOperations {
		List<byte> _buffer;

		public ByteBuffer(int capacity = 0) {
			_buffer = new List<byte>(capacity);
		}

		public ByteBuffer(IEnumerable<byte> data) {
			_buffer = new List<byte>(data);
		}

		public override long Size => _buffer.Count;


		public override void Delete(long offset, int count) {
			CommandManager.AddCommand(new DeleteCommand(this, (int)offset, count));
		}

		public override byte[] GetBytes(long offset, ref int count) {
			if (count + offset > Size)
				count = (int)(Size - offset);
			return _buffer.GetRange((int)offset, count).ToArray();
		}

		public override void Insert(long offset, byte[] data) {
			CommandManager.AddCommand(new InsertCommand(this, (int)offset, data));
		}

		public override void SetData(long offset, byte[] data) {
			CommandManager.AddCommand(new SetDataCommand(this, (int)offset, data));
		}

		public void InsertData(long offset, byte[] data) {
			_buffer.InsertRange((int)offset, data);
			OnSizeChanged();
		}

		public void DeleteRange(long offset, int count) {
			_buffer.RemoveRange((int)offset, count);
			OnSizeChanged();
		}

		public void SetBytes(long offset, byte[] data) {
			bool sizeChanged = data.Length + offset > _buffer.Count;
			if(sizeChanged)
				_buffer.AddRange(new byte[offset + data.Length - _buffer.Count]);

			for (int i = 0; i < data.Length; i++)
				_buffer[i] = data[i];
			if (sizeChanged)
				OnSizeChanged();
		}
	}
}

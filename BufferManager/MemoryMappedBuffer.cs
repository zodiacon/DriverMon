using BufferManager.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	class MemoryMappedBuffer : BufferBase, IBufferOperations {
		readonly MemoryMappedFile _mappedFile;
		readonly MemoryMappedViewAccessor _accessor;
		string _fileName;
		readonly bool _isReadOnly;
		byte[] _bytes = new byte[1 << 20];
		long _size;
		byte[] _copyBuffer = new byte[1 << 20];

		public MemoryMappedBuffer(string path, bool isReadOnly = false) {
			try {
				_mappedFile = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0,
					isReadOnly ? MemoryMappedFileAccess.Read : MemoryMappedFileAccess.ReadWrite);
				_isReadOnly = isReadOnly;
			}
			catch (Exception) {
				_mappedFile = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
				_isReadOnly = true;
			}
			_fileName = path;
			_accessor = _mappedFile.CreateViewAccessor(0, 0, _isReadOnly ? MemoryMappedFileAccess.Read : MemoryMappedFileAccess.ReadWrite);
			_size = new FileInfo(path).Length;
		}

		public MemoryMappedBuffer(long initialSize = 0) {
			_mappedFile = MemoryMappedFile.CreateNew(null, initialSize);
			_size = initialSize;
			_accessor = _mappedFile.CreateViewAccessor();
		}

		public override bool IsReadOnly => _isReadOnly;

		public override long Size => _size;

		public override void Delete(long offset, int count) {
			CommandManager.AddCommand(new DeleteCommand(this, offset, count));
		}

		public override byte[] GetBytes(long offset, ref int count) {
			if (offset + count > Size)
				count = (int)(Size - offset);
			if (count > _bytes.Length)
				Array.Resize(ref _bytes, count);

			_accessor.ReadArray(offset, _bytes, 0, count);
			return _bytes.Take(count).ToArray();
		}

		public override void Insert(long offset, byte[] data) {
			CommandManager.AddCommand(new InsertCommand(this, offset, data));
		}

		public override void SetData(long offset, byte[] data) {
			CommandManager.AddCommand(new SetDataCommand(this, offset, data));
		}

		public override void Dispose() {
			_accessor?.Dispose();
			_mappedFile?.Dispose();
		}

		public void InsertData(long offset, byte[] data) {
			Copy(offset, offset + data.Length);
			_accessor.WriteArray(offset, data, 0, data.Length);
			_size += data.Length;
			OnSizeChanged();
		}

		private void Copy(long from, long to) {
			if (from < to) {
				long count = Size - from;
				while (count > 0) {
					int n = count > _copyBuffer.Length ? _copyBuffer.Length : (int)count;
					_accessor.ReadArray(Size - n + from, _copyBuffer, 0, n);
					_accessor.WriteArray(Size - n + to, _copyBuffer, 0, n);

					from += n;
					to += n;
					count -= n;
				}
			}
			else {
				long count = Size - from;
				while (count > 0) {
					int n = count > _copyBuffer.Length ? _copyBuffer.Length : (int)count;
					_accessor.ReadArray(from, _copyBuffer, 0, n);
					_accessor.WriteArray(to, _copyBuffer, 0, n);

					from += n;
					to += n;
					count -= n;
				}
			}
		}

		public void DeleteRange(long offset, int count) {
			Copy(offset + count, offset);
			_size -= count;
			OnSizeChanged();
		}

		public void SetBytes(long offset, byte[] data) {
			_accessor.WriteArray(offset, data, 0, data.Length);
		}
	}
}

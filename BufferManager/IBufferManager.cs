using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	public interface IBufferManager : IDisposable {
		byte[] GetBytes(long offset, ref int count);
		long Size { get; }
		bool IsReadOnly { get; }

		event EventHandler SizeChanged;

		void Insert(long offset, byte[] data);
		void Delete(long offset, int count);
		void SetData(long offset, byte[] data);

		int UndoLevel { get; set; }

		void Undo();
		void Redo();
		bool CanUndo { get; }
		bool CanRedo { get; }
	}
}

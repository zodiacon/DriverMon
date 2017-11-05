using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	public enum EditMode {
		Overwrite,
		Insert
	}

	public interface IBufferEditor {
		EditMode EditMode { get; set; }
		long CaretOffset { get; set; }
		int WordSize { get; set; }
		int InputBase { get; set; }
		void UpdateDigit(byte value);
		ulong CurrentValue { get; }
		bool Backup();		// backup one digit
		void Flush();
		event EventHandler CaretMoved;
	}
}

using BufferManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexEditControl {
	public interface IHexEdit {
		void LoadFile(string path);
		void AttachToProcess(int pid);
        void AttachToBuffer(byte[] buffer, int startIndex = 0, int length = 0);
		void CreateNew();
		int WordSize { get; set; }
		int BytesPerLine { get; set; }
		IBufferEditor Editor { get; }
		IBufferManager BufferManager { get; }
	}
}

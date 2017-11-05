using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferManager {
	public interface IBufferOperations {
		void InsertData(long offset, byte[] data);
		void DeleteRange(long offset, int count);
		void SetBytes(long offset, byte[] data);
	}
}
